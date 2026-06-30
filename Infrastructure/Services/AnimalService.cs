using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Models;

namespace Infrastructure.Services;

public class AnimalService : IAnimalService
{
    private readonly IUnitOfWork _uow;
    public AnimalService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<PaginatedList<Animal>> GetAnimalsAsync(AnimalsFilter filter, PaginationParams pagination, CancellationToken ct = default)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (pagination is null)
            throw new ArgumentNullException(nameof(pagination));

        return await _uow.Animals.GetPagginatedListAsync(filter, pagination, ct);
    }
}
