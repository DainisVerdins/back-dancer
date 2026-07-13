using Application.Dtos.Animal;
using Application.Exceptions;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;

namespace Application.CORS.Queries;

public class GetAnimalWithImagesQuery : IRequest<AnimalDto>
{
    public required int AnimalId { get; init; }
}
public class GetAnimalWithImagesQueryHandler : IRequestHandler<GetAnimalWithImagesQuery, AnimalDto>
{
    private readonly IAnimalService _animalService;
    private readonly IMapper _mapper;

    public GetAnimalWithImagesQueryHandler(
       IAnimalService animalService, IMapper mapper)
    {
        _animalService = animalService;
        _mapper = mapper;
    }

    public async Task<AnimalDto> Handle(GetAnimalWithImagesQuery request, CancellationToken cancellationToken)
    {
        var result = await _animalService.GetAnimalWithImagesAsync(request.AnimalId, cancellationToken);
        if (result is null)
            throw new NotFoundException(ErrorMessages.GetApiErrorMessage(ApiErrorCode.EntityDoesNotExist));

        var output = _mapper.Map<AnimalDto>(result);

        return output;
    }
}
