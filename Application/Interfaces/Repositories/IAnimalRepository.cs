using Application.Entities.Animals;
using Application.Entities.Common;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IAnimalRepository : IGenericRepository<Animal>
{
    public Task<PaginatedList<Animal>> GetPagginatedListAsync(AnimalsFilter filter, PaginationParams paging , CancellationToken ct = default);
}
