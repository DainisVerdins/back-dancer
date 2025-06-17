using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Data;
using Infrastructure.Persistance.UnitOfWork;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddAutoMapper(typeof(Program).Assembly);

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        
        services.AddScoped<IPasswordService, PasswordService>();


        /* Persistance */
        services.AddDbContext<DataContext>(options =>
               options.UseSqlite(configuration.GetConnectionString("DefaultConnection"), db => db.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

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
