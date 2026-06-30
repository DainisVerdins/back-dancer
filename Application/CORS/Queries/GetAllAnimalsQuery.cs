using Application.Dtos;
using Application.Dtos.Animal;
using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.CORS.Queries;

public class GetAllAnimalsQuery : IRequest<BaseResponse<PaginatedList<AnimalDto>>>
{
    public required PaginationParams Paging { get; init; }

    public required AnimalFilterViewModel Filter { get; set; }
}
public class GetAllAnimalsQueryHandler : IRequestHandler<GetAllAnimalsQuery, BaseResponse<PaginatedList<AnimalDto>>>
{
    private readonly IAnimalService _animalService;
    private readonly IMapper _mapper;

    public GetAllAnimalsQueryHandler(
       IAnimalService animalService, IMapper mapper)
    {
        _animalService = animalService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<AnimalDto>>> Handle(GetAllAnimalsQuery request, CancellationToken cancellationToken)
    {
        if (request.Paging is null)
            throw new ArgumentNullException(nameof(request.Paging));

        if (request.Filter is null)
            throw new ArgumentNullException(nameof(request.Filter));

        var filter = _mapper.Map<AnimalsFilter>(request.Filter);

        var result = await _animalService.GetAnimalsAsync(filter, request.Paging, cancellationToken);

        var animalsDto = result.Items.Select(animal => {
            return _mapper.Map<AnimalDto>(animal);
        }).ToList();

        var output = new PaginatedList<AnimalDto>(animalsDto, result.TotalCount, request.Paging.PageNumber, request.Paging.PageSize);

        return new BaseResponse<PaginatedList<AnimalDto>>(output, HttpStatusCode.OK);
    }
}
