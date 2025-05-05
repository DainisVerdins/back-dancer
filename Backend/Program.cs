using Application;
using Persistence;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Infrastructure;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using WebApi.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApi;
public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        var loggerConfiguration = new LoggerConfiguration();
        if (builder.Environment.IsDevelopment())
        {
            loggerConfiguration = loggerConfiguration
                .MinimumLevel.Information()
                .WriteTo.Console();
        }
        else
        {
            loggerConfiguration = loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext();
        }
        var logger = loggerConfiguration.CreateLogger();
        try
        {
            Log.Information("Starting Backend App");
            // Add services to the container.
            // https://www.claudiobernasconi.ch/2022/01/28/how-to-use-serilog-in-asp-net-core-web-api/ for precise logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            // project dependencies in Clean Architecture pattern
            builder.Services
                .AddApplication()
                .AddPersistence(builder.Configuration)
                .AddInfrastructure();

            // add swagger
            builder.Services.AddSwaggerGen();
            builder.Services.AddEndpointsApiExplorer();
            // configure swagger
            builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

            // API versioning
            builder.Services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                                new HeaderApiVersionReader("x-api-version"),
                                                                new MediaTypeApiVersionReader("x-api-version"));
            })
                .AddApiExplorer(setup =>
                {
                    setup.GroupNameFormat = "'v'VVV";
                    setup.SubstituteApiVersionInUrl = true;
                });

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            builder.Services.AddCors(options =>
            {
                var allowedCordsOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Value;
                allowedCordsOrigins ??= "http://localhost:5173";
                options.AddPolicy("FrontendPolicy", builder => builder
                                   .WithOrigins(allowedCordsOrigins.Split(","))
                                   .AllowAnyHeader()
                                   .AllowAnyMethod()
                                   .AllowCredentials());
            });

            builder.Services.AddAuthorization();
            var authOptions = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

            if (authOptions is null)
                throw new Exception("Jwt setting was not provided!");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = authOptions.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });

            builder.Services.AddControllers();

            var app = builder.Build();


            //using (var scope = app.Services.CreateScope())
            //{
            //    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            //    await initializer.InitializeAsync();
            //}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(u =>
                {
                    u.RouteTemplate = "swagger/{documentName}/swagger.json";
                });
                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                    options.RoutePrefix = string.Empty;
                });
            }

            app.UseCors("FrontendPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.Information("Stopping Backend application");
            Log.CloseAndFlush();
        }
    }
}
