using Application.Dtos.Animal;
using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using MediatR;

namespace Application.CORS.Queries;

public class GetAllAnimalsQuery : IRequest<PaginatedList<AnimalDto>>
{
    public required PaginationParams Paging { get; init; }

    public required AnimalFilterViewModel Filter { get; set; }
}
public class GetAllAnimalsQueryHandler : IRequestHandler<GetAllAnimalsQuery, PaginatedList<AnimalDto>>
{
    private readonly IAnimalService _animalService;
    private readonly IMapper _mapper;

    public GetAllAnimalsQueryHandler(
       IAnimalService animalService, IMapper mapper)
    {
        _animalService = animalService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AnimalDto>> Handle(GetAllAnimalsQuery request, CancellationToken cancellationToken)
    {
        if (request.Paging is null)
            throw new ArgumentNullException(nameof(request.Paging), ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty));

        if (request.Filter is null)
            throw new ArgumentNullException(nameof(request.Filter), ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty));

        var filter = _mapper.Map<AnimalsFilter>(request.Filter);

        var result = await _animalService.GetAnimalsAsync(filter, request.Paging, cancellationToken);

        var animalsDto = result.Items.Select(animal =>
        {
            return _mapper.Map<AnimalDto>(animal);
        }).ToList();

        return new PaginatedList<AnimalDto>(animalsDto, result.TotalCount, request.Paging.PageNumber, request.Paging.PageSize);
    }
}
