using Application.Constants;
using Application.Entities;
using Application.Entities.Templates;
using AwesomeAssertions;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace Infrastructure.Tests.Services;

public class SendGridEmailServiceTests
{
    private readonly Mock<ISendGridClient> _clientMock;
    private readonly SendGridSettings _settings;
    private readonly SendGridEmailService _service;

    public SendGridEmailServiceTests()
    {
        _clientMock = new Mock<ISendGridClient>();

        _settings = new SendGridSettings
        {
            FromEmail = "from@test.com",
            FromName = "Test Sender",
            Templates = new Dictionary<string, string>
            {
                { EmailTemplate.UserInvite.ToString(), "template-id" }
            }
        };

        var options = Options.Create(_settings);
        _service = new SendGridEmailService(_clientMock.Object, options);
    }
    [Fact]
    public async Task SendSimpleEmailAsync_ShouldCallSendEmailAsync()
    {
        // Arrange
        var message = new SimpleEmailMessage
        {
            ToEmail = "user@test.com",
            Subject = "Subject",
            Body = "Body",
            HTMLBody = "<b>Body</b>"
        };

        _clientMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendGridMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        // Act
        await _service.SendSimpleEmailAsync(message, CancellationToken.None);

        // Assert
        _clientMock.Verify(x => x.SendEmailAsync(
            It.IsAny<SendGridMessage>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendSimpleEmailAsync_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Act
        Func<Task> action = () => _service.SendSimpleEmailAsync(null!);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendSimpleEmailAsync_ShouldThrowApplicationException_WhenSendFails()
    {
        // Arrange
        var message = new SimpleEmailMessage
        {
            ToEmail = "user@test.com",
            Subject = "Subject",
            Body = "Body"
        };

        _clientMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendGridMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(
                HttpStatusCode.BadRequest,
                new StringContent("SendGrid error"),
                null));

        // Act
        Func<Task> action = () => _service.SendSimpleEmailAsync(message);

        // Assert
        await action.Should()
            .ThrowAsync<ApplicationException>()
            .WithMessage("SendGrid error");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldCallSendEmailAsync()
    {
        // Arrange
        var message = new TemplateEmailMessage
        {
            ToEmail = "user@test.com",
            TemplateData = new InviteTemplateData
            {
                ExpiresAtText = DateTime.UtcNow.ToString()
            }
        };

        _clientMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendGridMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        // Act
        await _service.SendTemplateEmailAsync(
            message,
            EmailTemplate.UserInvite, CancellationToken.None);

        // Assert
        _clientMock.Verify(x => x.SendEmailAsync(
            It.Is<SendGridMessage>(m =>
                m.TemplateId == "template-id" &&
                m.From.Email == _settings.FromEmail &&
                m.Personalizations.Single().Tos.Single().Email == message.ToEmail),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Act
        Func<Task> action = () =>
            _service.SendTemplateEmailAsync(
                null!,
                EmailTemplate.UserInvite);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldThrowArgumentException_WhenTemplateIsNotConfigured()
    {
        // Arrange
        var service = new SendGridEmailService(
            _clientMock.Object,
            Options.Create(new SendGridSettings
            {
                FromEmail = "from@test.com",
                FromName = "Sender",
                Templates = new Dictionary<string, string>()
            }));

        var message = new TemplateEmailMessage
        {
            ToEmail = "user@test.com",
            TemplateData = new InviteTemplateData { ExpiresAtText = "John" }
        };

        // Act
        Func<Task> action = () =>
            service.SendTemplateEmailAsync(
                message,
                EmailTemplate.UserInvite);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldThrowApplicationException_WhenSendFails()
    {
        // Arrange
        var message = new TemplateEmailMessage
        {
            ToEmail = "user@test.com",
            TemplateData = new InviteTemplateData
            {
                ExpiresAtText = "John"
            }
        };

        _clientMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendGridMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(
                HttpStatusCode.BadRequest,
                new StringContent("Template error"),
                null));

        // Act
        Func<Task> action = () =>
            _service.SendTemplateEmailAsync(
                message,
                EmailTemplate.UserInvite);

        // Assert
        await action.Should()
            .ThrowAsync<ApplicationException>()
            .WithMessage("Template error");
    }
}
