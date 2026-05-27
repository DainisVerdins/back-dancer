using AutoMapper;
using AwesomeAssertions;
using Domain.Models;
using Infrastructure.Services;
using Infrastructure.Tests.Fakes;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Infrastructure.Tests.Services;

public class RoleServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RoleService _sut;

    public RoleServiceTests()
    {
        _userManagerMock = IdentityMockFactory.CreateUserManager();
        _roleManagerMock = IdentityMockFactory.CreateRoleManager();
        _mapperMock = new Mock<IMapper>();

        _sut = new RoleService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _mapperMock.Object
        );
    }

    #region CreateRoleAsync Tests

    [Fact]
    public async Task CreateRoleAsync_WithValidRole_ShouldInvokeRoleManager()
    {
        // Arrange
        var role = new Role { Name = "Editor" };
        _roleManagerMock.Setup(x => x.CreateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.CreateRoleAsync(role);

        // Assert
        _roleManagerMock.Verify(x => x.CreateAsync(role), Times.Once);
    }

    #endregion

    #region GetRoleByNameAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetRoleByNameAsync_WhenRoleNameIsNullOrEmpty_ShouldThrowArgumentException(string? invalidRoleName)
    {
        // Act
        var act = async () => await _sut.GetRoleByNameAsync(invalidRoleName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("roleName");
    }

    [Fact]
    public async Task GetRoleByNameAsync_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string roleName = "Guest";
        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.GetRoleByNameAsync(roleName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoleByNameAsync_WhenRoleExists_ShouldReturnMappedRole()
    {
        // Arrange
        string roleName = "Admin";
        var databaseRole = new Role { Name = roleName };
        var domainRole = new Role { Name = roleName };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(databaseRole);
        _mapperMock.Setup(x => x.Map<Role>(databaseRole))
            .Returns(domainRole);

        // Act
        var result = await _sut.GetRoleByNameAsync(roleName);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(domainRole);
    }

    #endregion

    #region GetRolesForUserAsync Tests

    [Fact]
    public async Task GetRolesForUserAsync_WhenUserIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _sut.GetRolesForUserAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Fact]
    public async Task GetRolesForUserAsync_WhenUserDoesNotExistInStore_ShouldReturnEmptyList()
    {
        // Arrange
        var user = new User { Id = 1 };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetRolesForUserAsync(user);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRolesForUserAsync_WhenUserHasAssignedRoles_ShouldReturnMappedRolesList()
    {
        // Arrange
        var user = new User { Id = 1 };
        var roleNames = new List<string> { "Admin", "Moderator" };
        var dbAdminRole = new Role { Name = "Admin" };
        var mappedAdminRole = new Role { Name = "Admin" };

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roleNames);

        // Setup one found role and one missing role in store for comprehensive evaluation
        _roleManagerMock.Setup(x => x.FindByNameAsync("Admin")).ReturnsAsync(dbAdminRole);
        _roleManagerMock.Setup(x => x.FindByNameAsync("Moderator")).ReturnsAsync((Role?)null);

        _mapperMock.Setup(x => x.Map<Role>(dbAdminRole)).Returns(mappedAdminRole);

        // Act
        var result = await _sut.GetRolesForUserAsync(user);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Admin");
    }

    #endregion

    #region IsUserInRoleAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task IsUserInRoleAsync_WhenRoleNameIsNullOrEmpty_ShouldThrowArgumentException(string? invalidRoleName)
    {
        // Arrange
        var user = new User();

        // Act
        var act = async () => await _sut.IsUserInRoleAsync(user, invalidRoleName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("roleName");
    }

    [Fact]
    public async Task IsUserInRoleAsync_WhenUserIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _sut.IsUserInRoleAsync(null!, "Admin");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Fact]
    public async Task IsUserInRoleAsync_WithValidArguments_ShouldReturnIdentityResult()
    {
        // Arrange
        var domainUser = new User { Id = 1 };
        var appUser = new User { Id = domainUser.Id };
        string roleName = "User";

        _mapperMock.Setup(x => x.Map<User>(domainUser)).Returns(appUser);
        _userManagerMock.Setup(x => x.IsInRoleAsync(appUser, roleName))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.IsUserInRoleAsync(domainUser, roleName);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region RoleExistsAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RoleExistsAsync_WhenRoleNameIsNullOrEmpty_ShouldThrowArgumentException(string? invalidRoleName)
    {
        // Act
        var act = async () => await _sut.RoleExistsAsync(invalidRoleName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("roleName");
    }

    [Fact]
    public async Task RoleExistsAsync_WhenCalled_ShouldReturnVerificationStatus()
    {
        // Arrange
        string roleName = "Administrator";
        _roleManagerMock.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RoleExistsAsync(roleName);

        // Assert
        result.Should().BeTrue();
        _roleManagerMock.Verify(x => x.RoleExistsAsync(roleName), Times.Once);
    }

    #endregion
}
