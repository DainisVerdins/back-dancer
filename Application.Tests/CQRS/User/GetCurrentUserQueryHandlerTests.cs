using Application.CORS.Queries;
using Application.Interfaces.Services;
using AwesomeAssertions;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Security.Claims;

namespace Application.Tests.CQRS.User;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    private readonly DefaultHttpContext _httpContext;

    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _roleServiceMock = new Mock<IRoleService>();

        _httpContext = new DefaultHttpContext();

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(_httpContext);

        _sut = new GetCurrentUserQueryHandler(
            _userServiceMock.Object,
            _httpContextAccessorMock.Object,
            _roleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndRoleClaimExists_ShouldReturnUserDto()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            UserName = "Kurland",
            Email = "kurland@test.com"
        };

        _userServiceMock
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Role, "Admin")
            ]));

        _roleServiceMock.Setup(x => x.GetRolesForUserAsync(user))
            .ReturnsAsync([new() { RoleCode = "Admin" }]);

        // Act
        var result = await _sut.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.UserName.Should().Be("Kurland");
        result.Email.Should().Be("kurland@test.com");
    }

    [Fact]
    public async Task Handle_WhenRoleClaimDoesNotExist_ShouldReturnNoRoleSelected()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            UserName = "Kurland",
            Email = "kurland@test.com"
        };

        _userServiceMock
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity());

        _roleServiceMock.Setup(x => x.GetRolesForUserAsync(user))
            .ReturnsAsync([new() { RoleCode = "Admin" }]);

        // Act
        var result = await _sut.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.ActiveRole.Should().Be("NoRoleSelected");
    }

    [Fact]
    public async Task Handle_WhenHttpContextIsNull_ShouldReturnNoRoleSelected()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            UserName = "Kurland",
            Email = "kurland@test.com"
        };

        _userServiceMock
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null!);

        _roleServiceMock.Setup(x => x.GetRolesForUserAsync(user))
            .ReturnsAsync([new() { RoleCode = "Admin" }]);

        // Act
        var result = await _sut.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.ActiveRole.Should().Be("NoRoleSelected");
    }

    [Fact]
    public async Task Handle_WhenUserNameAndEmailAreNull_ShouldReturnEmptyStrings()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            UserName = null,
            Email = null
        };

        _userServiceMock
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _roleServiceMock.Setup(x => x.GetRolesForUserAsync(user))
            .ReturnsAsync([new() { RoleCode = "Admin" }]);

        // Act
        var result = await _sut.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.UserName.Should().Be("");
        result.Email.Should().Be("");
        result.ActiveRole.Should().Be("NoRoleSelected");
    }
}
