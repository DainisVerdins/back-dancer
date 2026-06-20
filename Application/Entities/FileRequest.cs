namespace Application.Entities;

public class FileRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
