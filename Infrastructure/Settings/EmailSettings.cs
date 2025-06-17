using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Settings;

public class EmailSettings
{
    [EmailAddress]
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
}
