using Application.Constants;
using Application.Entities;
using Application.Interfaces.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class SendGridEmailService : IEmailService
{

    private readonly SendGridSettings _settings;
    public SendGridEmailService(IOptions<SendGridSettings> option)
    {
        _settings = option.Value;
    }

    public Task SendTemplateEmailAsync(TemplateEmailMessage messageToSend, EmailTemplate emailTemplate, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
