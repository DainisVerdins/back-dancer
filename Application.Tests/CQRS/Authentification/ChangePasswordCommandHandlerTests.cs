using Application.CORS.Commands.Authentication;
using Application.Exceptions;
using Application.Interfaces.Services;
using AwesomeAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Net;

namespace Application.Tests.CQRS.Authentification;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _sut = new ChangePasswordCommandHandler(_userServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnUnauthorizedResponse()
    {
        // Arrange
        _userServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync((Domain.Models.User?)null);

        var command = new ChangePasswordCommand("OldPass123!", "NewPass123!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.ErrorMessages.Should().Contain(ErrorMessages.GetMessage(ErrorCode.UserNotFound));
        _userServiceMock.Verify(x => x.ChangePasswordAsync(It.IsAny<Domain.Models.User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenIdentityResultFails_ShouldReturnBadRequestWithErrors()
    {
        // Arrange
        var user = new Domain.Models.User { Id = 1, UserName = "Tester" };
        var identityErrors = new List<IdentityError>
        {
            new() { Description = "Password requires a non-alphanumeric character." },
            new() { Description = "Password too short." }
        };

        _userServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _userServiceMock.Setup(x => x.ChangePasswordAsync(user, "OldPass123!", "InvalidNewPass"))
            .ReturnsAsync(IdentityResult.Failed([.. identityErrors]));

        var command = new ChangePasswordCommand("OldPass123!", "InvalidNewPass");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.ErrorMessages.Should().HaveCount(2);
        result.ErrorMessages.Should().Contain("Password too short.");
        result.Data.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var user = new Domain.Models.User { Id = 1, UserName = "Tester" };

        _userServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _userServiceMock.Setup(x => x.ChangePasswordAsync(user, "OldPass123!", "ValidNewPass123!"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand("OldPass123!", "ValidNewPass123!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.ErrorMessages.Should().BeEmpty();
        result.Data.Should().Be(Unit.Value);

        _userServiceMock.Verify(x => x.ChangePasswordAsync(user, "OldPass123!", "ValidNewPass123!"), Times.Once);
    }
}
