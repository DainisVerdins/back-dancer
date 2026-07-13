using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using MediatR;

namespace Application.CORS.Animal;

public class DeleteAnimalCommand : IRequest
{
    public required int Id { get; init; }
}

public class DeleteAnimalCommandHandler : IRequestHandler<DeleteAnimalCommand>
{

    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fileService;
    private readonly IAnimalService _animalService;

    public DeleteAnimalCommandHandler(IUnitOfWork uow, IFileStorageService fileService, IAnimalService animalService)
    {
        _uow = uow;
        _fileService = fileService;
        _animalService = animalService;
    }

    public async Task Handle(DeleteAnimalCommand request, CancellationToken ct)
    {
        if (request.Id < 1)
            throw new ValidationException("Id can not be lower than 1");

        var animalToRemove = await _animalService.GetAnimalWithImagesAsync(request.Id, ct);
        if (animalToRemove is null)
            throw new NotFoundException(ErrorMessages.GetMessage(ErrorCode.NotFound));

        foreach (var img in animalToRemove.Images)
        {
            await _fileService.DeleteFileAsync(img.Key, ct);
            _uow.AnimalImages.Remove(img);
        }


        _uow.Animals.Remove(animalToRemove);
        await _uow.SaveChangesAsync(ct);
    }
}
