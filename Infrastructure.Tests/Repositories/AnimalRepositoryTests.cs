using Application.Entities.Animals;
using Application.Entities.Common;
using Domain.Models;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Tests.Fixtures;

namespace Infrastructure.Tests.Repositories;

public class AnimalRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public AnimalRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetPagginatedListAsync
    [Fact]
    public async Task GetPagginatedListAsync_FiltersCorrectly()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        context.Animals.RemoveRange(context.Animals);
        await context.SaveChangesAsync(CancellationToken.None);
        context.Animals.AddRange(new List<Animal>
        {
            new Animal { Name = "Nagatoro", Breed = "Human", Gender = Domain.Enums.Gender.Female },
            new Animal { Name = "Luna", Breed = "Cat", Gender = Domain.Enums.Gender.Female }
        });
        await context.SaveChangesAsync(CancellationToken.None);
        var repo = new AnimalRepository(context);
        var filter = new AnimalsFilter { NameSearchTerm = "Nag" };
        var paging = new PaginationParams { PageNumber = 0, PageSize = 10 };

        // Act
        var result = await repo.GetPagginatedListAsync(filter, paging, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Nagatoro", result.Items.First().Name);
    }

    [Theory]
    [InlineData("dateofbirth", false, "Murka")] 
    [InlineData("dateofbirth", true, "Nagatoro")] 
    public async Task GetPagginatedListAsync_SortsCorrectly(string sortBy, bool isDescending, string expectedFirstName)
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Animals.RemoveRange(context.Animals);

        context.Animals.AddRange(new List<Animal>
        {
            new Animal { Name = "Nagatoro", DateOfBirth = new DateOnly(2026, 6, 9) },
            new Animal { Name = "Murka", DateOfBirth = new DateOnly(2003, 12, 19) }
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var repo = new AnimalRepository(context);
        var filter = new AnimalsFilter();
        var paging = new PaginationParams
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = sortBy,
            IsDescending = isDescending
        };

        // Act
        var result = await repo.GetPagginatedListAsync(filter, paging, CancellationToken.None);

        // Assert
        Assert.Equal(expectedFirstName, result.Items.First().Name);
    }

    #endregion

    #region GetAnimalWithImagesAsync
    [Fact]
    public async Task GetAnimalByIdWithImagesAsync_ShouldReturnAnimalWithImages_WhenIdIsValid()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var animalId = 10;
        var animal = new Animal
        {
            Id = animalId,
            Name = "Barsik",
            Images = new List<AnimalImage> 
            {
                new AnimalImage { Url = "test1.jpg" },
                new AnimalImage { Url = "test2.jpg" }
            }
        };

        context.Animals.Add(animal);
        await context.SaveChangesAsync(CancellationToken.None);

        var repo = new AnimalRepository(context);

        // Act
        var result = await repo.GetAnimalByIdWithImagesAsync(animalId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Barsik", result!.Name);
        Assert.NotNull(result.Images);
        Assert.Equal(2, result.Images.Count);
        Assert.Contains(result.Images, i => i.Url == "test1.jpg");
    }

    [Fact]
    public async Task GetAnimalByIdWithImagesAsync_ShouldThrowArgumentException_WhenIdIsZero()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repo = new AnimalRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => repo.GetAnimalByIdWithImagesAsync(0, CancellationToken.None));
    }

    [Fact]
    public async Task GetAnimalByIdWithImagesAsync_ShouldReturnNull_WhenAnimalDoesNotExist()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repo = new AnimalRepository(context);

        // Act
        var result = await repo.GetAnimalByIdWithImagesAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
    #endregion
    #region GetPaginatedListWithImagesAsync
    [Fact]
    public async Task GetPaginatedListWithImagesAsync_ShouldReturnPaginatedListWithImages()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Animals.RemoveRange(context.Animals);

        // Добавляем животных с изображениями
        context.Animals.AddRange(new List<Animal>
        {
            new Animal { Name = "Cat1", Images = new List<AnimalImage> { new AnimalImage { Url = "cat1.jpg" } } },
            new Animal { Name = "Cat2", Images = new List<AnimalImage> { new AnimalImage { Url = "cat2.jpg" } } }
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var repo = new AnimalRepository(context);
        var paging = new PaginationParams { PageNumber = 0, PageSize = 10 };

        // Act
        var result = await repo.GetPaginatedListWithImagesAsync(paging, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, animal => {
            Assert.NotNull(animal.Images);
            Assert.NotEmpty(animal.Images);
        });
    }

    [Fact]
    public async Task GetPaginatedListWithImagesAsync_ShouldWorkWithEmptyDb()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Animals.RemoveRange(context.Animals);
        await context.SaveChangesAsync(CancellationToken.None);

        var repo = new AnimalRepository(context);
        var paging = new PaginationParams { PageNumber = 0, PageSize = 10 };

        // Act
        var result = await repo.GetPaginatedListWithImagesAsync(paging, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
    #endregion
}
