using Application.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"), db => db.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = false;
        })
           .AddRoles<Role>()
           .AddEntityFrameworkStores<DataContext>();

        return services;
    }
}
