namespace Infrastructure.Settings;

public class SendGridSettings
{
    public const string SectionName = "SendGridSettings";
    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
