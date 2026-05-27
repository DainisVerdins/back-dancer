using AwesomeAssertions;
using Domain.Models;
using Infrastructure.Services;
using Infrastructure.Tests.Fakes;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Infrastructure.Tests.Services;

public class PasswordServiceTests
{

    private PasswordService CreateSut(IList<IPasswordValidator<User>> validators)
    {
        var fakeUserManager = new FakeUserManager(validators);

        return new PasswordService(fakeUserManager);
    }

    #region ValidatePasswordAsync tests
    [Fact]
    public async Task ValidatePasswordAsync_WithNoValidators_ShouldReturnEmptyList()
    {
        // Arrange
        var sut = CreateSut(new List<IPasswordValidator<User>>());

        // Act
        var result = await sut.ValidatePasswordAsync("SomePassword123!");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WhenValidatorSucceeds_ShouldReturnEmptyList()
    {
        // Arrange
        var validatorMock = new Mock<IPasswordValidator<User>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UserManager<User>>(), It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(new List<IPasswordValidator<User>> { validatorMock.Object });

        // Act
        var result = await sut.ValidatePasswordAsync("StrongPassword123!");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WhenValidatorFails_ShouldReturnErrors()
    {
        // Arrange
        var errors = new[]
        {
            new IdentityError { Description = "Password too short" },
            new IdentityError { Description = "Password requires uppercase" }
        };

        var validatorMock = new Mock<IPasswordValidator<User>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UserManager<User>>(), It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var sut = CreateSut(new List<IPasswordValidator<User>> { validatorMock.Object });

        // Act
        var result = await sut.ValidatePasswordAsync("weak");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Password too short");
        result.Should().Contain("Password requires uppercase");
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithNullPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var sut = CreateSut(new List<IPasswordValidator<User>>());

        // Act
        var act = async () => await sut.ValidatePasswordAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var sut = CreateSut(new List<IPasswordValidator<User>>());

        // Act
        var act = async () => await sut.ValidatePasswordAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
    #endregion
}
