using Application.Dtos.Animal;
using Application.Entities.Common;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.CORS.Queries;

public class GetPublicAnimalsPagedQuery : IRequest<BaseResponse<PaginatedList<PublicAnimalDto>>>
{
    public required PaginationParams Paging { get; init; }
}

public class GetPublicAnimalsPagedQueryHandler : IRequestHandler<GetPublicAnimalsPagedQuery, BaseResponse<PaginatedList<PublicAnimalDto>>>
{
    private readonly IAnimalService _animalService;
    private readonly IMapper _mapper;

    public GetPublicAnimalsPagedQueryHandler(
       IAnimalService animalService, IMapper mapper)
    {
        _animalService = animalService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<PublicAnimalDto>>> Handle(GetPublicAnimalsPagedQuery request, CancellationToken cancellationToken)
    {
        if (request.Paging is null)
            throw new ArgumentNullException(nameof(request.Paging));


        var result = await _animalService.GetPublicAnimalsAsync(request.Paging, cancellationToken);

        var animalsDto = result.Items.Select(animal =>
        {
            return _mapper.Map<PublicAnimalDto>(animal);
        }).ToList();

        var output = new PaginatedList<PublicAnimalDto>(animalsDto, result.TotalCount, request.Paging.PageNumber, request.Paging.PageSize);

        return new BaseResponse<PaginatedList<PublicAnimalDto>>(output, HttpStatusCode.OK);
    }
}