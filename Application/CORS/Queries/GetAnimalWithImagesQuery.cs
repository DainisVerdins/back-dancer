using Application.Dtos.Animal;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.CORS.Queries;

public class GetAnimalWithImagesQuery : IRequest<BaseResponse<AnimalDto>>
{
    public required int AnimalId { get; init; }
}
public class GetAnimalWithImagesQueryHandler : IRequestHandler<GetAnimalWithImagesQuery, BaseResponse<AnimalDto>>
{
    private readonly IAnimalService _animalService;
    private readonly IMapper _mapper;

    public GetAnimalWithImagesQueryHandler(
       IAnimalService animalService, IMapper mapper)
    {
        _animalService = animalService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<AnimalDto>> Handle(GetAnimalWithImagesQuery request, CancellationToken cancellationToken)
    {
        var result = await _animalService.GetAnimalWithImagesAsync(request.AnimalId, cancellationToken);
        if (result is null)
            return new BaseResponse<AnimalDto>(null, ErrorMessages.GetApiErrorMessage(ApiErrorCode.EntityDoesNotExist), HttpStatusCode.NotFound);

        var output = _mapper.Map<AnimalDto>(result);

        return new BaseResponse<AnimalDto>(output);
    }
}
