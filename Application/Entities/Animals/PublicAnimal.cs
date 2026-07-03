using Domain.Enums;

namespace Application.Entities.Animals;

public class PublicAnimal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Url { get; set; } = string.Empty;
}
