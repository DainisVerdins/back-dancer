using Application.CORS.Animal;
using Application.Entities;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.Animal;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Tests.CQRS.Animal;

public class CreateAnimalCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IFileStorageService> _fileServiceMock;
    private readonly IMapper _mapper;
    private readonly CreateAnimalCommandHandler _handler;
    private readonly Mock<IAnimalRepository> _animalRepositoryMock;
    public CreateAnimalCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _animalRepositoryMock = new Mock<IAnimalRepository>();
        _fileServiceMock = new Mock<IFileStorageService>();
        _uowMock.Setup(u => u.Animals).Returns(_animalRepositoryMock.Object);

        var configExpression = new MapperConfigurationExpression();
        configExpression.CreateMap<CreateAnimalViewModel, Domain.Models.Animal>();
        var config = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _handler = new CreateAnimalCommandHandler(_uowMock.Object, _mapper, _fileServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAnimalAndUploadPhotos()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var command = new CreateAnimalCommand
        {
            Model = new CreateAnimalViewModel
            {
                Name = "Buddy",
                NewPhotos = new List<IFormFile> { mockFile.Object }
            }
        };

        _fileServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<FileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://s3.com/image.jpg");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileServiceMock.Verify(s => s.UploadFileAsync(It.IsAny<FileRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.Animals.AddAsync(It.IsAny<Domain.Models.Animal>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}