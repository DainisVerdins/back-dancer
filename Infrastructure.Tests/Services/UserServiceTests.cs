using Application.Interfaces.Services;
using AwesomeAssertions;
using Domain.Models;
using Infrastructure.Services;
using Infrastructure.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace Infrastructure.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userManagerMock = IdentityMockFactory.CreateUserManager();
        _roleServiceMock = new Mock<IRoleService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _sut = new UserService(
            _userManagerMock.Object,
            _roleServiceMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    #region CheckPasswordAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CheckPasswordAsync_WhenPasswordIsNullOrEmpty_ShouldThrowArgumentException(string? password)
    {
        var act = async () => await _sut.CheckPasswordAsync(new User(), password!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CheckPasswordAsync_WhenUserIsNull_ShouldThrowArgumentNullException()
    {
        var act = async () => await _sut.CheckPasswordAsync(null!, "Password123!");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CheckPasswordAsync_WithValidArgs_ShouldReturnUserManagerResult()
    {
        var user = new User { UserName = "Kettler" };
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Password123!"))
            .ReturnsAsync(true);

        var result = await _sut.CheckPasswordAsync(user, "Password123!");

        result.Should().BeTrue();
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WhenUserAlreadyExists_ShouldReturnTrueImmediately()
    {
        var user = new User { UserName = "ExistingUser" };
        _userManagerMock.Setup(x => x.FindByNameAsync("ExistingUser"))
            .ReturnsAsync(user);

        var result = await _sut.CreateUserAsync(user, "Password123!");

        result.Should().BeTrue();
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserDoesNotExist_ShouldCreateAndReturnStatus()
    {
        var user = new User { UserName = "NewUser" };
        _userManagerMock.Setup(x => x.FindByNameAsync("NewUser")).ReturnsAsync((User?)null);
        _userManagerMock.Setup(x => x.CreateAsync(user, "Password123!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.CreateUserAsync(user, "Password123!");

        result.Should().BeTrue();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WhenIdIsLessThanOne_ShouldThrowArgumentException()
    {
        var act = async () => await _sut.GetUserByIdAsync(0);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        var user = new User { Id = 5 };
        _userManagerMock.Setup(x => x.FindByIdAsync("5")).ReturnsAsync(user);

        var result = await _sut.GetUserByIdAsync(5);

        result.Should().Be(user);
    }

    #endregion

    #region AddRoleToUserAsync Tests

    [Fact]
    public async Task AddRoleToUserAsync_WhenRoleDoesNotExist_ShouldThrowException()
    {
        var user = new User { UserName = "Tester" };
        _roleServiceMock.Setup(x => x.RoleExistsAsync("FakeRole")).ReturnsAsync(false);

        var act = async () => await _sut.AddRoleToUserAsync(user, "FakeRole");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Role with name FakeRole does not exit!");
    }

    [Fact]
    public async Task AddRoleToUserAsync_WithValidUserAndRole_ShouldInvokeAddToRoleAsync()
    {
        var user = new User { UserName = "Tester" };
        _roleServiceMock.Setup(x => x.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.FindByNameAsync("Tester")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.AddRoleToUserAsync(user, "Admin");

        _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    #endregion

    #region GetClaimsForAccessTokenByUserIdAsync Tests

    [Fact]
    public async Task GetClaimsForAccessTokenByUserIdAsync_WhenUserHasNoRole_ShouldThrowException()
    {
        var user = new User { Id = 1, UserName = "NoRoleUser", Email = "test@test.com" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());
        _roleServiceMock.Setup(x => x.GetRolesForUserAsync(user)).ReturnsAsync(new List<Role>());

        var act = async () => await _sut.GetClaimsForAccessTokenByUserIdAsync(1);

        await act.Should().ThrowAsync<Exception>().WithMessage("User does not have role!");
    }

    [Fact]
    public async Task GetClaimsForAccessTokenByUserIdAsync_WithValidUserAndRole_ShouldReturnAggregatedClaims()
    {
        var user = new User { Id = 1, UserName = "StandardUser", Email = "user@test.com" };
        var extraClaims = new List<Claim> { new("CustomClaim", "Value") };
        var roles = new List<Role> { new() { Name = "Moderator" } };

        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(extraClaims);
        _roleServiceMock.Setup(x => x.GetRolesForUserAsync(user)).ReturnsAsync(roles);

        var result = await _sut.GetClaimsForAccessTokenByUserIdAsync(1);

        result.Should().NotBeEmpty();
        result.Any(c => c.Type == ClaimTypes.Role && c.Value == "Moderator").Should().BeTrue();
        result.Any(c => c.Type == "CustomClaim").Should().BeTrue();
    }

    #endregion

    #region GetCurrentUserAsync Tests

    [Fact]
    public async Task GetCurrentUserAsync_WhenHttpContextHasNoUser_ShouldReturnNull()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null!);

        var result = await _sut.GetCurrentUserAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserGuidClaim_ShouldReturnFoundUser()
    {
        var userId = 2;
        var claims = new List<Claim> { new("UserId", userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var appUser = new User { Id = 10, UserName = "ActiveUser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(appUser);

        var result = await _sut.GetCurrentUserAsync();

        result.Should().NotBeNull();
        result!.UserName.Should().Be("ActiveUser");
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordsAreIdentical_ShouldThrowException()
    {
        var user = new User();
        var act = async () => await _sut.ChangePasswordAsync(user, "SamePassword1!", "SamePassword1!");

        await act.Should().ThrowAsync<Exception>().WithMessage("new and old password are same");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidInputs_ShouldReturnIdentityResult()
    {
        var user = new User();
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "OldPass1!", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ChangePasswordAsync(user, "OldPass1!", "NewPass1!");

        result.Succeeded.Should().BeTrue();
    }

    #endregion
}
