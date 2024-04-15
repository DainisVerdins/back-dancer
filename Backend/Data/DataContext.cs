using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class DataContext : DbContext
{
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }
}
