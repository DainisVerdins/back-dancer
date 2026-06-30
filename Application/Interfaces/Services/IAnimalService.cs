using Application.Entities.Animals;
using Application.Entities.Common;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface IAnimalService
{
    Task<PaginatedList<Animal>> GetAnimalsAsync(AnimalsFilter filter, PaginationParams pagination, CancellationToken ct = default);
    Task<Animal?> GetAnimalWithImagesAsync(int animalId, CancellationToken ct = default);
}
