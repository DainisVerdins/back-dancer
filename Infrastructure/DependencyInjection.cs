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
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODExMTE2ODAwIiwiaWF0IjoiMTc3OTY0MjYzMCIsImFjY291bnRfaWQiOiIwMTllNWFmNzBmNDE3MzBiYmU3N2YxZTFjZjQ5ZDI1ZSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa3NkZmYwZjlkcmNqcWI3OTJrczNoamI4Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.PaH0n-yHuCJGauhVbZOGSbo3ZPvLT89l9Ibn6hSWNY5sTDmCKL5CDZDoaxUUpBMgOlHoog2kFl7a6CivY9-rl6lva4dD5I5W5qHkaJRFEYbe5MNJi6Abe9Wl_sRCVnBH3k7-ln9RQntBaYvqFBjCo1V_vQFeAicRlCibzWm9NgoqNx8MZ8SKB2lK8TCVIb_G0kr0jUz3BfXn4V1t5fPEWvZvPiQFDUXctaAyKjq7b6KxrVqQTYRV-vSAq2MnypaaCi_b5IMzQ0xGEV2WksT0rerl0UrBtZEVeKtAyh_d7eQYLWcsj_VhVw38V8ZVsJb7h8MoPW4ZPUiPHZE8o5tGuQ";
        }, typeof(Program).Assembly);

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        
        services.AddScoped<IPasswordService, PasswordService>();


        /* Persistance */
        services.AddScoped<DatabaseInitializer>();
        services.AddDbContext<DataContext>(options =>
               options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

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

        services.Configure<InitialUserSettings>(configuration.GetSection("InitialUserCredentials"));

        return services;
    }
}
