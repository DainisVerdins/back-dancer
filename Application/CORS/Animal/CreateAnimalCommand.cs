using Application.Entities.Common;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using Domain.Models;
using MediatR;

namespace Application.CORS.Animal;

public class CreateAnimalCommand : IRequest<BaseResponse<int>>
{
    public required CreateAnimalViewModel Model { get; init; }
}

public class CreateAnimalCommandHandler : IRequestHandler<CreateAnimalCommand, BaseResponse<int>>
{

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileService;

    public CreateAnimalCommandHandler(IUnitOfWork uow, IMapper mapper, IFileStorageService fileService)
    {
        _uow = uow;
        _mapper = mapper;
        _fileService = fileService;
    }

    public async Task<BaseResponse<int>> Handle(CreateAnimalCommand request, CancellationToken ct)
    {
        var animal = _mapper.Map<Domain.Models.Animal>(request.Model);

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

            animal.Images.Add(new AnimalImage
            {
                Url = url,
                IsMain = animal.Images.Count == 0,
                Key = key
            });
        }

        await _uow.Animals.AddAsync(animal, ct);
        await _uow.SaveChangesAsync(ct);

        return new BaseResponse<int>(animal.Id);
    }
}
