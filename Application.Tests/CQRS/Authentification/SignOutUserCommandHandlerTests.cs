using Application.CORS.Commands.Authentication;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AwesomeAssertions;
using Domain.Models;
using MediatR;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Tests.CQRS.Authentification;

public class SignOutUserCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly SignOutUserCommandHandler _sut;

    public SignOutUserCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

        _unitOfWorkMock.Setup(x => x.RefreshTokens).Returns(_refreshTokenRepoMock.Object);

        _sut = new SignOutUserCommandHandler(
            _userServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Fail Path Tests

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowNotFoundException()
    {
        // Arrange
        _userServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync((Domain.Models.User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exceptions.NotFoundException>(() => _sut.Handle(new SignOutUserCommand(), CancellationToken.None));
    }

    #endregion

    #region Success Path Tests

    [Fact]
    public async Task Handle_WhenUserHasActiveTokens_ShouldRevokeAllActiveTokensAndReturnOk()
    {
        // Arrange
        var user = new Domain.Models.User { Id = 123, UserName = "CourlandCreator" };
        _userServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(user);

        // Prepare active tokens that belong to the user
        var activeTokens = new List<RefreshToken>
        {
            new() { Id = 1, UserId = 123, Token = "token-aaa", IsRevoked = false },
            new() { Id = 2, UserId = 123, Token = "token-bbb", IsRevoked = false }
        };

        // We setup the Find expression matcher to capture the tracking evaluation and return our list
        _refreshTokenRepoMock
            .Setup(x => x.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .Returns(activeTokens);

        // Act
        await _sut.Handle(new SignOutUserCommand(), CancellationToken.None);

        // Verify side-effects: both tokens inside our active list must be modified to true!
        activeTokens.All(t => t.IsRevoked).Should().BeTrue();

        // Verify changes are flushed to storage engine
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoActiveTokens_ShouldStillSaveAndReturnOk()
    {
        // Arrange
        var user = new Domain.Models.User { Id = 456, UserName = "EmptyTokenUser" };
        _userServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(user);

        // Return empty list if no active token matching constraints exist
        _refreshTokenRepoMock
            .Setup(x => x.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .Returns(new List<RefreshToken>());

        // Act
        await _sut.Handle(new SignOutUserCommand(), CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
