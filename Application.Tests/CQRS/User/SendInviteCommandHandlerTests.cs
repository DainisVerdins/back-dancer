using Application.CORS.Commands;
using Application.Entities;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Settings;
using AwesomeAssertions;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.CQRS.User;

public class SendInviteCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUserInviteRepository> _inviteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenHasherService> _tokenHasherServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;

    private readonly SendInviteCommandHandler _sut;

    public SendInviteCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _inviteRepositoryMock = new Mock<IUserInviteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenHasherServiceMock = new Mock<ITokenHasherService>();
        _emailServiceMock = new Mock<IEmailService>();

        _unitOfWorkMock
            .Setup(x => x.UserInvites)
            .Returns(_inviteRepositoryMock.Object);

        var domainSettings = Options.Create(
            new DomainSettings
            {
                FrontendDomain = "https://frontend.example.com",
                ApplicationDomain = "https://api.example.com"
            });

        _sut = new SendInviteCommandHandler(
            _userServiceMock.Object,
            _unitOfWorkMock.Object,
            _tokenHasherServiceMock.Object,
            _emailServiceMock.Object,
            domainSettings);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new SendInviteCommand
        {
            Email = "existing@example.com",
            Roles = ["Admin"],
            InvitedByUserId = 1
        };

        var existingUser = new Domain.Models.User
        {
            Id = 10,
            Email = command.Email
        };

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var action = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ConflictException>();

        _inviteRepositoryMock.Verify(
            x => x.GetPendingByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _tokenHasherServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);

        _inviteRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<UserInvite>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            x => x.SendTemplateEmailAsync(
                It.IsAny<TemplateEmailMessage>(),
                It.IsAny<Application.Constants.EmailTemplate>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPendingInviteAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new SendInviteCommand
        {
            Email = "invited@example.com",
            Roles = ["Volunteer"],
            InvitedByUserId = 1
        };

        var existingInvite = new UserInvite
        {
            Id = 15,
            Email = command.Email,
            Token = "existing-hash",
            Status = InviteStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _inviteRepositoryMock
            .Setup(x => x.GetPendingByEmailAsync(
                command.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvite);

        // Act
        var action = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ConflictException>();

        _tokenHasherServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);

        _inviteRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<UserInvite>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            x => x.SendTemplateEmailAsync(
                It.IsAny<TemplateEmailMessage>(),
                It.IsAny<Application.Constants.EmailTemplate>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_ShouldCreateInviteSaveAndSendEmail()
    {
        // Arrange
        var command = new SendInviteCommand
        {
            Email = "new@example.com",
            Roles = ["Admin", "Volunteer"],
            InvitedByUserId = 5
        };

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _inviteRepositoryMock
            .Setup(x => x.GetPendingByEmailAsync(
                command.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvite?)null);

        _tokenHasherServiceMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("hashed-token");

        UserInvite? createdInvite = null;

        _inviteRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<UserInvite>(),
                It.IsAny<CancellationToken>()))
            .Callback<UserInvite, CancellationToken>(
                (invite, _) =>
                {
                    createdInvite = invite;
                    invite.Id = 42;
                })
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(x => x.SendTemplateEmailAsync(
                It.IsAny<TemplateEmailMessage>(),
                Application.Constants.EmailTemplate.UserInvite,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // TODO : REFIGURE UNIT TEST AND CREATED AT 
        var beforeExecution = DateTimeOffset.UtcNow;

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        var afterExecution = DateTimeOffset.UtcNow;

        // Assert
        result.Should().Be(42);

        createdInvite.Should().NotBeNull();
        createdInvite!.Email.Should().Be(command.Email);
        createdInvite.Token.Should().Be("hashed-token");
        createdInvite.InvitedByUserId.Should().Be(command.InvitedByUserId);
        createdInvite.Status.Should().Be(InviteStatus.Pending);

        createdInvite.Roles.Should()
            .BeEquivalentTo(command.Roles);

        createdInvite.ExpiresAt
            .Should()
            .Be(createdInvite.CreatedAt.AddHours(48));

        _tokenHasherServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Once);

        _inviteRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<UserInvite>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendTemplateEmailAsync(
                It.IsAny<TemplateEmailMessage>(),
                Application.Constants.EmailTemplate.UserInvite,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailSendingFails_ShouldPropagateException()
    {
        // Arrange
        var command = new SendInviteCommand
        {
            Email = "new@example.com",
            Roles = ["Volunteer"],
            InvitedByUserId = 5
        };

        _userServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((Domain.Models.User?)null);

        _inviteRepositoryMock
            .Setup(x => x.GetPendingByEmailAsync(
                command.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvite?)null);

        _tokenHasherServiceMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("hashed-token");

        _inviteRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<UserInvite>(),
                It.IsAny<CancellationToken>()))
            .Callback<UserInvite, CancellationToken>(
                (invite, _) =>
                {
                    invite.Id = 100;
                })
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(x => x.SendTemplateEmailAsync(
                It.IsAny<TemplateEmailMessage>(),
                Application.Constants.EmailTemplate.UserInvite,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException("Email service failed."));

        // Act
        var action = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}