using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    { }
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    //protected override void OnModelCreating(ModelBuilder builder)
    //{
    //    builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    //}

}
