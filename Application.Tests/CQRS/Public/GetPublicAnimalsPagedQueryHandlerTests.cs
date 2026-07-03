using Application.CORS.Queries;
using Application.Dtos.Animal;
using Application.Entities.Animals;
using Application.Entities.Common;
using Application.Interfaces.Services;
using AutoMapper;
using Moq;
using System.Net;

namespace Application.Tests.CQRS.Public;

public class GetPublicAnimalsPagedQueryHandlerTests
{
    private readonly Mock<IAnimalService> _animalServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetPublicAnimalsPagedQueryHandler _handler;

    public GetPublicAnimalsPagedQueryHandlerTests()
    {
        _animalServiceMock = new Mock<IAnimalService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetPublicAnimalsPagedQueryHandler(_animalServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsSuccessfulResponse()
    {
        // Arrange
        var paging = new PaginationParams { PageNumber = 0, PageSize = 10 };
        var query = new GetPublicAnimalsPagedQuery { Paging = paging };

        var animals = new List<PublicAnimal> { new PublicAnimal() };
        var paginatedResult = new PaginatedList<PublicAnimal>(animals, 1, 0, 10);

        _animalServiceMock.Setup(s => s.GetPublicAnimalsAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        _mapperMock.Setup(m => m.Map<PublicAnimalDto>(It.IsAny<object>()))
            .Returns(new PublicAnimalDto { Name = "Test" });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Single(result.Data.Items);
        Assert.Equal("Test", result.Data.Items.First().Name);
        _animalServiceMock.Verify(s => s.GetPublicAnimalsAsync(paging, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPagingIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var query = new GetPublicAnimalsPagedQuery { Paging = null! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
