namespace Application.Entities;

public class SimpleEmailMessage
{
    public string ToEmail { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HTMLBody { get; set; } = string.Empty;
}
