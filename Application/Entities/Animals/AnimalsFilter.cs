using Domain.Enums;

namespace Application.Entities.Animals;

public class AnimalsFilter
{
    public string? NameSearchTerm { get; set; }
    public Gender? Gender { get; set; }
    public string? BreedSearchTerm { get; set; }

    public AnimalSize? Size { get; set; }
    public Temperament? Temperament { get; set; }
    public AnimalStatus? Status { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public bool? IsSterilized { get; set; }
    public bool? IsVaccinated { get; set; }
}
