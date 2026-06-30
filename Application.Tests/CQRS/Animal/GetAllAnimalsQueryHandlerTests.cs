using Application.CORS.Queries;
using Application.Dtos.Animal;
using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using AwesomeAssertions;
using Moq;
using System.Net;

namespace Application.Tests.CQRS.Animal;

public class GetAllAnimalsQueryHandlerTests
{
    private readonly Mock<IAnimalService> _animalServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllAnimalsQueryHandler _handler;

    public GetAllAnimalsQueryHandlerTests()
    {
        _animalServiceMock = new Mock<IAnimalService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllAnimalsQueryHandler(_animalServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResponse_WhenDataIsValid()
    {
        // Arrange
        var paging = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var filterVm = new AnimalFilterViewModel();
        var query = new GetAllAnimalsQuery { Paging = paging, Filter = filterVm };

        var animals = new List<Domain.Models.Animal> { new Domain.Models.Animal { Id = 1, Name = "Test" } };
        var paginatedList = new PaginatedList<Domain.Models.Animal>(animals, 1, 1, 10);
        var animalDto = new AnimalDto { Id = 1, Name = "Test" };

        _mapperMock.Setup(m => m.Map<AnimalsFilter>(filterVm)).Returns(new AnimalsFilter());
        _animalServiceMock.Setup(s => s.GetAnimalsAsync(It.IsAny<AnimalsFilter>(), paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedList);
        _mapperMock.Setup(m => m.Map<AnimalDto>(It.IsAny<Domain.Models.Animal>())).Returns(animalDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Items.Count.Should().Be(1);
        result.Data.Items.First().Name.Should().Be("Test");

        _animalServiceMock.Verify(s => s.GetAnimalsAsync(It.IsAny<AnimalsFilter>(), paging, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenPagingIsNull()
    {
        // Arrange
        var query = new GetAllAnimalsQuery { Paging = null!, Filter = new AnimalFilterViewModel() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenFilterIsNull()
    {
        // Arrange
        var query = new GetAllAnimalsQuery { Paging = new PaginationParams(), Filter = null! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
