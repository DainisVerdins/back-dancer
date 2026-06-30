using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Services;
using Moq;

namespace Infrastructure.Tests.Services;

public class AnimalServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IAnimalRepository> _animalRepoMock;
    private readonly AnimalService _service;

    public AnimalServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _animalRepoMock = new Mock<IAnimalRepository>();

        _uowMock.Setup(u => u.Animals).Returns(_animalRepoMock.Object);

        _service = new AnimalService(_uowMock.Object);
    }

    [Fact]
    public async Task GetAnimalsAsync_ShouldCallRepository_WhenParametersAreValid()
    {
        // Arrange
        var filter = new AnimalsFilter();
        var paging = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PaginatedList<Animal>(new List<Animal>(), 0, 1, 10);

        _animalRepoMock
            .Setup(r => r.GetPagginatedListAsync(filter, paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetAnimalsAsync(filter, paging, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Same(expectedResult, result);
        _animalRepoMock.Verify(r => r.GetPagginatedListAsync(filter, paging, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAnimalsAsync_ShouldThrowArgumentNullException_WhenFilterIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetAnimalsAsync(null!, new PaginationParams(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAnimalsAsync_ShouldThrowArgumentNullException_WhenPaginationIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetAnimalsAsync(new AnimalsFilter(), null!, CancellationToken.None));
    }

    #region GetAnimalWithImagesAsync
    [Fact]
    public async Task GetAnimalWithImagesAsync_ShouldCallRepository_WhenIdIsValid()
    {
        // Arrange
        int animalId = 5;
        var expectedAnimal = new Animal { Id = animalId, Name = "Test Animal" };

        _animalRepoMock
            .Setup(r => r.GetAnimalByIdWithImagesAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnimal);

        // Act
        var result = await _service.GetAnimalWithImagesAsync(animalId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAnimal.Id, result!.Id);
        _animalRepoMock.Verify(r => r.GetAnimalByIdWithImagesAsync(animalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAnimalWithImagesAsync_ShouldThrowArgumentException_WhenIdIsZero()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAnimalWithImagesAsync(0, CancellationToken.None));

        _animalRepoMock.Verify(r => r.GetAnimalByIdWithImagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion
}
