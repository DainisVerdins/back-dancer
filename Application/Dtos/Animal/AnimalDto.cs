namespace Application.Dtos.Animal;

using Domain.Enums;

public class AnimalDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string Description { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public AnimalSize Size { get; set; }
    public Temperament Temperament { get; set; }
    public AnimalStatus Status { get; set; }

    public DateOnly? DateOfBirth { get; set; }
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;

    public bool IsSterilized { get; set; }
    public bool IsVaccinated { get; set; }

    public List<AnimalImageDto> Images { get; set; } = [];
}

