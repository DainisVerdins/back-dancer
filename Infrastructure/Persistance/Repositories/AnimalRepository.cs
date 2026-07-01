using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class AnimalRepository : GenericRepository<Animal>, IAnimalRepository
{
    public AnimalRepository(DbContext context) : base(context)
    {
    }

    public Task<PaginatedList<Animal>> GetPagginatedListAsync(AnimalsFilter filter, PaginationParams paging, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.NameSearchTerm))
            query = query.Where(a => a.Name.Contains(filter.NameSearchTerm));

        if (!string.IsNullOrWhiteSpace(filter.BreedSearchTerm))
            query = query.Where(a => a.Breed != null && a.Breed.Contains(filter.BreedSearchTerm));

        if (filter.Gender.HasValue)
            query = query.Where(a => a.Gender == filter.Gender);

        if (filter.Size.HasValue)
            query = query.Where(a => a.Size == filter.Size);

        if (filter.IsSterilized.HasValue)
            query = query.Where(a => a.IsSterilized == filter.IsSterilized);

        if (filter.IsVaccinated.HasValue)
            query = query.Where(a => a.IsVaccinated == filter.IsVaccinated);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status);
        if (filter.Temperament.HasValue)
            query = query.Where(a => a.Temperament == filter.Temperament);
        // TODO: implement proper AdmissionDate filtering
        //if (filter.AdmissionDate.HasValue)
        //    query = query.Where(a => a.AdmissionDate == filter.AdmissionDate);


        query = paging.SortBy?.ToLower() switch
        {
            "name" => paging.IsDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            "breed" => paging.IsDescending ? query.OrderByDescending(a => a.Breed) : query.OrderBy(a => a.Breed),
            "date" => paging.IsDescending ? query.OrderByDescending(a => a.AdmissionDate) : query.OrderBy(a => a.AdmissionDate),
            "gender" => paging.IsDescending ? query.OrderByDescending(a => a.Gender) : query.OrderBy(a => a.Gender),
            "dateofbirth" => paging.IsDescending ? query.OrderByDescending(a => a.DateOfBirth) : query.OrderBy(a => a.DateOfBirth),
            _ => query.OrderBy(a => a.Id)
        };


        return PaginatedList<Animal>.CreateAsync(query, paging.PageNumber, paging.PageSize, ct);
    }

    public async Task<Animal?> GetAnimalByIdWithImagesAsync(int animalId, CancellationToken ct = default)
    {
        if (animalId == 0)
            throw new ArgumentException(ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty), nameof(animalId));

        return await _dbSet
            .AsNoTracking()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == animalId, ct);
    }
}
