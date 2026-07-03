using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Models;

namespace Infrastructure.Services;

public class AnimalService : IAnimalService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    public AnimalService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }
    public async Task<PaginatedList<Animal>> GetAnimalsAsync(AnimalsFilter filter, PaginationParams pagination, CancellationToken ct = default)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (pagination is null)
            throw new ArgumentNullException(nameof(pagination));

        return await _uow.Animals.GetPagginatedListAsync(filter, pagination, ct);
    }

    public async Task<Animal?> GetAnimalWithImagesAsync(int animalId, CancellationToken ct = default)
    {
        if (animalId == 0)
            throw new ArgumentException(ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty), nameof(animalId));

        return await _uow.Animals.GetAnimalByIdWithImagesAsync(animalId, ct);
    }

    public async Task<PaginatedList<PublicAnimal>> GetPublicAnimalsAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        if (pagination is null)
            throw new ArgumentNullException(nameof(pagination));

        var pagedAnimals = await _uow.Animals.GetPaginatedListWithImagesAsync(pagination, ct);

        var publicAnimals = pagedAnimals.Items.Select(a => _mapper.Map<PublicAnimal>(a)).ToList();

        return new PaginatedList<PublicAnimal>(publicAnimals, pagedAnimals.TotalCount, pagination.PageNumber, pagination.PageSize);
    }
}
