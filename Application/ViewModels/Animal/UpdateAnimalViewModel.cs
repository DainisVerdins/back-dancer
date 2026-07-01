using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.ViewModels.Animal;

public class UpdateAnimalViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string Description { get; set; } = string.Empty;
    public string HealthNote { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public AnimalSize Size { get; set; }
    public Temperament Temperament { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool IsSterilized { get; set; }
    public bool IsVaccinated { get; set; }
    public List<IFormFile> NewPhotos { get; set; } = [];
    public List<int> ExistingPhotoIds { get; set; } = [];
}
