using Application.CORS.Queries;
using Application.Dtos.Animal;
using Application.Interfaces.Services;
using AutoMapper;
using Moq;
using System.Net;

namespace Application.Tests.CQRS.Animal;

public class GetAnimalWithImagesQueryHandlerTests
{
    private readonly Mock<IAnimalService> _animalServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAnimalWithImagesQueryHandler _handler;

    public GetAnimalWithImagesQueryHandlerTests()
    {
        _animalServiceMock = new Mock<IAnimalService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAnimalWithImagesQueryHandler(_animalServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAnimalExists()
    {
        // Arrange
        var animalId = 1;
        var query = new GetAnimalWithImagesQuery { AnimalId = animalId };
        var animal = new Domain.Models.Animal { Id = animalId, Name = "Nagatoro" };
        var animalDto = new AnimalDto { Id = animalId, Name = "Nagatoro" };

        _animalServiceMock
            .Setup(s => s.GetAnimalWithImagesAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mapperMock
            .Setup(m => m.Map<AnimalDto>(animal))
            .Returns(animalDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(animalDto.Name, result.Data.Name);
        _mapperMock.Verify(m => m.Map<AnimalDto>(animal), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        // Arrange
        var animalId = 99;
        var query = new GetAnimalWithImagesQuery { AnimalId = animalId };

        _animalServiceMock
            .Setup(s => s.GetAnimalWithImagesAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Animal?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Data);
        Assert.NotEmpty(result.ErrorMessages);
    }
}
