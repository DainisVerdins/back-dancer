using Domain.Interfaces;

namespace Domain.Models;

public class AnimalImage : IEntity
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int AnimalId { get; set; }
    public Animal? Animal { get; set; }
}
