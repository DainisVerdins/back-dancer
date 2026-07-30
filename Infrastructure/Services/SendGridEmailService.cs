using Application.Constants;
using Application.Entities;
using Application.Interfaces.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Services;

public class SendGridEmailService : IEmailService
{

    private readonly SendGridSettings _settings;
    private readonly SendGridClient _client;
    private readonly EmailAddress _from;
    public SendGridEmailService(IOptions<SendGridSettings> option)
    {
        _settings = option.Value;
        _client = new SendGridClient(_settings.ApiKey);
        _from = new EmailAddress(_settings.FromEmail, _settings.FromName);
    }

    public async Task SendSimpleEmailAsync(SimpleEmailMessage message, CancellationToken token = default)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        var msg = MailHelper.CreateSingleEmail(_from, new EmailAddress(message.ToEmail), message.Subject, message.Body, message.HTMLBody);
        var response = await _client.SendEmailAsync(msg, token);

        if (!response.IsSuccessStatusCode)
        {
            var output = await response.Body.ReadAsStringAsync(token);
            throw new ApplicationException(output);
        }
    }

    public async Task SendTemplateEmailAsync(TemplateEmailMessage messageToSend, EmailTemplate emailTemplate, CancellationToken token = default)
    {
        if (messageToSend is null)
            throw new ArgumentNullException(nameof(messageToSend));

        var templateId = _settings.Templates.GetValueOrDefault(emailTemplate.ToString());

        if (string.IsNullOrEmpty(templateId))
            throw new ArgumentException($"Template not configured: {emailTemplate}");

        var msg = MailHelper.CreateSingleTemplateEmail(_from, new EmailAddress(messageToSend.ToEmail), templateId, messageToSend.TemplateData);
        var response = await _client.SendEmailAsync(msg, token);

        if (!response.IsSuccessStatusCode)
        {
            var output = await response.Body.ReadAsStringAsync(token);
            throw new ApplicationException(output);
        }

    }
}
