using Application.CORS.Animal;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using Moq;
using System.Net;

namespace Application.Tests.CQRS.Animal;

public class DeleteAnimalCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IFileStorageService> _fileServiceMock = new();
    private readonly Mock<IAnimalService> _animalServiceMock = new();
    private readonly DeleteAnimalCommandHandler _handler;

    public DeleteAnimalCommandHandlerTests()
    {
        var imageRepoMock = new Mock<IAnimalImageRepository>();
        var animalRepoMock = new Mock<IAnimalRepository>();

        _uowMock.Setup(u => u.AnimalImages).Returns(imageRepoMock.Object);
        _uowMock.Setup(u => u.Animals).Returns(animalRepoMock.Object);

        _handler = new DeleteAnimalCommandHandler(
            _uowMock.Object, _fileServiceMock.Object, _animalServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        // Arrange
        var command = new DeleteAnimalCommand { Id = 99 };
        _animalServiceMock.Setup(s => s.GetAnimalWithImagesAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Animal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldDeleteImagesAndAnimal_WhenExists()
    {
        // Arrange
        var animalId = 1;
        var image = new AnimalImage { Id = 10, Key = "test-key" };
        var animal = new Domain.Models.Animal { Id = animalId, Images = new List<AnimalImage> { image } };

        _animalServiceMock.Setup(s => s.GetAnimalWithImagesAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        // Act
        var result = await _handler.Handle(new DeleteAnimalCommand { Id = animalId }, CancellationToken.None);

        // Assert
        _fileServiceMock.Verify(s => s.DeleteFileAsync("test-key", It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.AnimalImages.Remove(image), Times.Once);
        _uowMock.Verify(u => u.Animals.Remove(animal), Times.Once);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenIdIsInvalid()
    {
        // Arrange
        var command = new DeleteAnimalCommand { Id = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
