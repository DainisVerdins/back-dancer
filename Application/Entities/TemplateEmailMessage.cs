using Application.Entities.Templates;

namespace Application.Entities;

public class TemplateEmailMessage
{
    public string ToEmail { get; set; } = string.Empty;
    public IEmailTemplateData? TemplateData { get; set; }
}
