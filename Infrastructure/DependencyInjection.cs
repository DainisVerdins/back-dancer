using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
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

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IPasswordService, PasswordService>();


        /* Persistance */
        services.AddScoped<DatabaseInitializer>();
        services.AddDbContext<DataContext>(options =>
               options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

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

        services.Configure<S3Settings>(configuration.GetSection(S3Settings.SectionName));
        services.AddAWSService<IAmazonS3>(new AWSOptions
        {
            Credentials = new BasicAWSCredentials(
                configuration["S3Settings:AccessKey"],
                configuration["S3Settings:SecretKey"]),
                    Region = Amazon.RegionEndpoint.GetBySystemName(
                configuration["S3Settings:Region"])
        });
        services.AddScoped<IFileStorageService, S3FileStorageService>();

        services.AddScoped<IAnimalService, AnimalService>();
        services.AddScoped<ITokenHasherService, TokenHasherService>();

        services.Configure<SendGridSettings>(configuration.GetSection(SendGridSettings.SectionName));

        return services;
    }
}
