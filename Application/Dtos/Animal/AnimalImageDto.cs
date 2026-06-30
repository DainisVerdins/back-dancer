namespace Application.Dtos.Animal;

public class AnimalImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}
