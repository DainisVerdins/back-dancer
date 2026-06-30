using Application.Entities.Animals;
using Application.Entities.Common;
using Domain.Models;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Tests.Repositories;

public class AnimalRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public AnimalRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPagginatedListAsync_FiltersCorrectly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
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
}
