using Application.CORS.Animal;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Application.Tests.CQRS.Animal;

public class UpdateAnimalCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IFileStorageService> _fileServiceMock = new();
    private readonly Mock<IAnimalService> _animalServiceMock = new();
    private readonly UpdateAnimalCommandHandler _handler;

    public UpdateAnimalCommandHandlerTests()
    {
        var imgRepoMock = new Mock<IAnimalImageRepository>();
        var animalsRepoMock = new Mock<IAnimalRepository>();
        _uowMock.Setup(u => u.AnimalImages).Returns(imgRepoMock.Object);
        _uowMock.Setup(u => u.Animals).Returns(animalsRepoMock.Object);
        _handler = new UpdateAnimalCommandHandler(
            _uowMock.Object, _mapperMock.Object, _fileServiceMock.Object, _animalServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveOldImages_WhenIdsAreMissingInExistingList()
    {
        // 1. Arrange
        var animalId = 1;
        var imageToKeep = new AnimalImage { Id = 10, Key = "key-1" };
        var imageToRemove = new AnimalImage { Id = 11, Key = "key-2" };

        var animal = new Domain.Models.Animal
        {
            Id = animalId,
            Images = new List<AnimalImage> { imageToKeep, imageToRemove }
        };

        var command = new UpdateAnimalCommand
        {
            Model = new UpdateAnimalViewModel
            {
                Id = animalId,
                ExistingPhotoIds = new List<int> { 10 },
                NewPhotos = new List<IFormFile>()
            }
        };

        _animalServiceMock.Setup(s => s.GetAnimalWithImagesAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        // 2. Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert
        _fileServiceMock.Verify(s => s.DeleteFileAsync("key-2", It.IsAny<CancellationToken>()), Times.Once);

        _uowMock.Verify(u => u.AnimalImages.Remove(imageToRemove), Times.Once);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddNewImage_WhenNewPhotosProvided()
    {
        // Arrange
        var animal = new Domain.Models.Animal { Id = 1, Images = new List<AnimalImage>() };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var command = new UpdateAnimalCommand
        {
            Model = new UpdateAnimalViewModel
            {
                Id = 1,
                ExistingPhotoIds = new List<int>(),
                NewPhotos = new List<IFormFile> { fileMock.Object }
            }
        };

        _animalServiceMock.Setup(s => s.GetAnimalWithImagesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileServiceMock.Verify(s => s.UploadFileAsync(It.IsAny<Entities.FileRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Single(animal.Images);
    }
}
