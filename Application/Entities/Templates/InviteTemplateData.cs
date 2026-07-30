namespace Application.Entities.Templates;

public class InviteTemplateData : IEmailTemplateData
{
    public string InviteLink { get; set; } = string.Empty;
    public string ExpiresAtText { get; set; } = string.Empty;
}
