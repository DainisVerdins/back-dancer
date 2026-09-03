using Application.CORS.Commands;
using Application.Entities;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AwesomeAssertions;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Security.Claims;

namespace Application.Tests.CQRS.User;

public class AcceptInviteCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserInviteRepository> _userInviteRepositoryMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITokenHasherService> _tokenHasherServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;

    private readonly AcceptInviteCommandHandler _sut;

    public AcceptInviteCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userInviteRepositoryMock = new Mock<IUserInviteRepository>();
        _userServiceMock = new Mock<IUserService>();
        _tokenHasherServiceMock = new Mock<ITokenHasherService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _unitOfWorkMock
            .Setup(x => x.UserInvites)
            .Returns(_userInviteRepositoryMock.Object);

        _sut = new AcceptInviteCommandHandler(
            _unitOfWorkMock.Object,
            _userServiceMock.Object,
            _tokenHasherServiceMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenInviteIsValid_ShouldCreateUserAssignRolesAcceptInviteAndReturnToken()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "plain-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "user@test.com",
            Token = tokenHash,
            Roles = ["User", "Manager"],
            Status = InviteStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(47)
        };

        var claims = new List<Claim>();

        var tokenResponse = new TokenResponse
        {
            Token = "access-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(invite.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                command.Password))
            .Callback<Domain.Models.User, string>((user, _) =>
            {
                user.Id = 10;
            })
            .ReturnsAsync(IdentityResult.Success);

        _userServiceMock
            .Setup(x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userServiceMock
            .Setup(x => x.GetClaimsForAccessTokenByUserIdAsync(10))
            .ReturnsAsync(claims);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(claims))
            .Returns(tokenResponse);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.AccessTokenExpiresAt.Should().Be(tokenResponse.ExpiresAt);

        invite.Status.Should().Be(InviteStatus.Accepted);

        _tokenHasherServiceMock.Verify(
            x => x.Hash(command.Token),
            Times.Once);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.GetUserByEmailAsync(invite.Email),
            Times.Once);

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.Is<Domain.Models.User>(u =>
                    u.Email == invite.Email &&
                    u.UserName == invite.Email &&
                    u.EmailConfirmed),
                command.Password),
            Times.Once);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "User"),
            Times.Once);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "Manager"),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.GetClaimsForAccessTokenByUserIdAsync(10),
            Times.Once);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(claims),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInviteIsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "invalid-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-invalid-token";

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvite?)null);

        // Act
        var act = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Invitation was not found.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(command.Token),
            Times.Once);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.GetUserByEmailAsync(
                It.IsAny<string>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<IEnumerable<Claim>>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_WhenInviteIsAlreadyAccepted_ShouldThrowConflictException()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "valid-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "user@test.com",
            Token = tokenHash,
            Roles = ["User"],
            Status = InviteStatus.Accepted,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(46)
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        // Act
        var act = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Invitation has already been accepted.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(command.Token),
            Times.Once);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.GetUserByEmailAsync(
                It.IsAny<string>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<IEnumerable<Claim>>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_WhenInviteIsExpired_ShouldThrowConflictException()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "expired-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-expired-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "user@test.com",
            Token = tokenHash,
            Roles = ["User"],
            Status = InviteStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-49),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        // Act
        var act = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Invitation has expired.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(command.Token),
            Times.Once);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.GetUserByEmailAsync(
                It.IsAny<string>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<IEnumerable<Claim>>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "valid-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "existing@test.com",
            Token = tokenHash,
            Roles = ["User"],
            Status = InviteStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(47)
        };

        var existingUser = new Domain.Models.User
        {
            Id = 10,
            Email = invite.Email,
            UserName = invite.Email
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(invite.Email))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage(
                $"User with email '{invite.Email}' already exists.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(command.Token),
            Times.Once);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.GetUserByEmailAsync(invite.Email),
            Times.Once);

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<IEnumerable<Claim>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldThrowValidationExceptionAndRollbackTransaction()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "valid-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "user@test.com",
            Token = tokenHash,
            Roles = ["User"],
            Status = InviteStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(47)
        };

        var identityError = new IdentityError
        {
            Code = "PasswordTooShort",
            Description = "Password is too short."
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(invite.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                command.Password))
            .ReturnsAsync(
                IdentityResult.Failed(identityError));

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Password is too short.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(command.Token),
            Times.Once);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.GetUserByEmailAsync(invite.Email),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                command.Password),
            Times.Once);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<IEnumerable<Claim>>()),
            Times.Never);

        invite.Status.Should().Be(InviteStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenRoleAssignmentFails_ShouldThrowValidationExceptionAndRollbackTransaction()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "valid-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "user@test.com",
            Token = tokenHash,
            Roles = ["Admin", "Manager"],
            Status = InviteStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(47)
        };

        var identityError = new IdentityError
        {
            Code = "RoleAssignmentFailed",
            Description = "Failed to assign role."
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(invite.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                command.Password))
            .Callback<Domain.Models.User, string>((user, _) =>
            {
                user.Id = 10;
            })
            .ReturnsAsync(IdentityResult.Success);

        _userServiceMock
            .Setup(x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        _userServiceMock
            .Setup(x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "Manager"))
            .ReturnsAsync(
                IdentityResult.Failed(identityError));

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Failed to assign role.");

        _userServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                command.Password),
            Times.Once);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "Admin"),
            Times.Once);

        _userServiceMock.Verify(
            x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "Manager"),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<IEnumerable<Claim>>()),
            Times.Never);

        invite.Status.Should().Be(InviteStatus.Pending);
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenTokenIsEmpty_ShouldThrowArgumentException(
    string token)
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = token,
            Password = "Password123!"
        };

        // Act
        var act = async () =>
            await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Invitation token is required.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenPasswordIsEmpty_ShouldThrowArgumentException(
    string password)
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "valid-token",
            Password = password
        };

        // Act
        var act = async () =>
            await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Password is required.");

        _tokenHasherServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);

        _userInviteRepositoryMock.Verify(
            x => x.GetByTokenHashAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldRollbackTransaction()
    {
        // Arrange
        var command = new AcceptInviteCommand
        {
            Token = "valid-token",
            Password = "Password123!"
        };

        var tokenHash = "hashed-token";

        var invite = new UserInvite
        {
            Id = 1,
            Email = "user@test.com",
            Token = tokenHash,
            Roles = ["User"],
            Status = InviteStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(47)
        };

        _tokenHasherServiceMock
            .Setup(x => x.Hash(command.Token))
            .Returns(tokenHash);

        _userInviteRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(invite.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<Domain.Models.User>(),
                command.Password))
            .Callback<Domain.Models.User, string>((user, _) => user.Id = 10)
            .ReturnsAsync(IdentityResult.Success);

        _userServiceMock
            .Setup(x => x.AddRoleToUserByRoleNameAsync(
                It.IsAny<Domain.Models.User>(),
                "User"))
            .ReturnsAsync(IdentityResult.Success);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () =>
            await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Database error");

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        invite.Status.Should().Be(InviteStatus.Accepted);
    }
}
