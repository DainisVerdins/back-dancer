using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.CORS.Animal;

public class DeleteAnimalCommand : IRequest<BaseResponse<Unit>>
{
    public required int Id { get; init; }
}

public class DeleteAnimalCommandHandler : IRequestHandler<DeleteAnimalCommand, BaseResponse<Unit>>
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

    public async Task<BaseResponse<Unit>> Handle(DeleteAnimalCommand request, CancellationToken ct)
    {
        if (request.Id < 1)
            throw new ArgumentException("Id can not be lower than 1");

        var animalToRemove = await _animalService.GetAnimalWithImagesAsync(request.Id, ct);
        if (animalToRemove is null)
            return new BaseResponse<Unit>(Unit.Value, ErrorMessages.GetApiErrorMessage(ApiErrorCode.EntityDoesNotExist), System.Net.HttpStatusCode.NotFound);

        foreach (var img in animalToRemove.Images)
        {
            await _fileService.DeleteFileAsync(img.Key, ct);
            _uow.AnimalImages.Remove(img);
        }


        _uow.Animals.Remove(animalToRemove);
        await _uow.SaveChangesAsync(ct);

        return new BaseResponse<Unit>(Unit.Value, HttpStatusCode.OK);
    }
}
