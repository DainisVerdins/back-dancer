using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using Domain.Models;
using MediatR;

namespace Application.CORS.Animal;

public class UpdateAnimalCommand : IRequest
{
    public required UpdateAnimalViewModel Model { get; init; }
}

public class UpdateAnimalCommandHandler : IRequestHandler<UpdateAnimalCommand>
{

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileService;
    private readonly IAnimalService _animalService;

    public UpdateAnimalCommandHandler(IUnitOfWork uow, IMapper mapper, IFileStorageService fileService, IAnimalService animalService)
    {
        _uow = uow;
        _mapper = mapper;
        _fileService = fileService;
        _animalService = animalService;
    }

    public async Task Handle(UpdateAnimalCommand request, CancellationToken ct)
    {

        var animalToUpdate = await _animalService.GetAnimalWithImagesAsync(request.Model.Id, ct);
        if (animalToUpdate is null)
            throw new NotFoundException(ErrorMessages.GetApiErrorMessage(ApiErrorCode.EntityDoesNotExist));

        _mapper.Map(request.Model, animalToUpdate);

        var imagesToRemove = animalToUpdate.Images
            .Where(i => !request.Model.ExistingPhotoIds.Contains(i.Id))
            .ToList();

        foreach (var img in imagesToRemove)
        {
            await _fileService.DeleteFileAsync(img.Key, ct);
            animalToUpdate.Images.Remove(img);
            _uow.AnimalImages.Remove(img);
        }

        foreach (var photo in request.Model.NewPhotos)
        {
            using var stream = photo.OpenReadStream();
            var key = Guid.NewGuid().ToString();
            var url = await _fileService.UploadFileAsync(new Entities.FileRequest
            {
                FileStream = stream,
                Key = key,
                ContentType = photo.ContentType,
                FileName = photo.FileName

            }, ct);

            animalToUpdate.Images.Add(new AnimalImage
            {
                Url = url,
                IsMain = animalToUpdate.Images.Count == 0,
                Key = key
            });
        }

        _uow.Animals.Update(animalToUpdate);
        await _uow.SaveChangesAsync(ct);
    }
}
