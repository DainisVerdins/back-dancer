using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Data;

public class DataContext : IdentityDbContext<User, Role, int>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
