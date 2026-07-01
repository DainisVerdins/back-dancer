using Application.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class AnimalImageRepository : GenericRepository<AnimalImage>, IAnimalImageRepository
{
    public AnimalImageRepository(DbContext context) : base(context)
    {
    }
}
