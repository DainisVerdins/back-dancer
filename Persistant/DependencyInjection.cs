using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using Persistence.Identity;
namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"), db => db.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));

        services.AddTransient<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = false;
        })
           .AddRoles<AppRole>()
           .AddEntityFrameworkStores<DataContext>();

        return services;
    }
}
