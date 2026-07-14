using Application.Constants;
using Application.CORS.Commands;
using Application.Entities;
using Application.Interfaces.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Net;
using System.Security.Claims;

namespace Application.Tests.CQRS.User;

public class SelectUserRoleCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<UserManager<Domain.Models.User>> _userManagerMock;

    private readonly SelectUserRoleCommandHandler _sut;

    public SelectUserRoleCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _userManagerMock = CreateUserManagerMock();

        _sut = new SelectUserRoleCommandHandler(
            _userServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object);
    }

    private static Mock<UserManager<Domain.Models.User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<Domain.Models.User>>();

        return new Mock<UserManager<Domain.Models.User>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_ShouldReturnAccessToken()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            UserName = "Kurland",
            Email = "test@test.com"
        };

        var command = new SelectUserRoleCommand
        {
            RoleCode = UserRole.Admin
        };

        var additionalClaims = new List<Claim>
        {
            new("Department", "IT")
        };

        var tokenResponse = new TokenResponse
        {
            Token = "access-token"
        };

        _userServiceMock
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.IsInRoleAsync(user, UserRole.Admin))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetClaimsAsync(user))
            .ReturnsAsync(additionalClaims);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
            .Returns(tokenResponse);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(tokenResponse);
    }
}
