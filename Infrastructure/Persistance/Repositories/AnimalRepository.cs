using Application.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class AnimalRepository : GenericRepository<Animal>, IAnimalRepository
{
    public AnimalRepository(DbContext context) : base(context)
    {
    }
}
