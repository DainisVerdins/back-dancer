using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Models;

public class Animal : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string Description { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public AnimalSize Size { get; set; }
    public Temperament Temperament { get; set; }
    public AnimalStatus Status { get; set; } = AnimalStatus.SearchingForHome;

    public DateTime? DateOfBirth { get; set; }
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;

    public bool IsSterilized { get; set; }
    public bool IsVaccinated { get; set; }

    public List<AnimalImage> Images { get; set; } = [];
}
