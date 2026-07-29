using Application.Constants;
using Application.Entities;

namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendTemplateEmailAsync(TemplateEmailMessage messageToSend, EmailTemplate emailTemplate, CancellationToken token = default);
}
