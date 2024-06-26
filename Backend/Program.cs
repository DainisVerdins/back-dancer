using Application;
using Backend.Configuration;
using Backend.Data;
using Backend.MappingProfiles;
using Backend.Middleware;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Persistence;
using Presentation;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();

try
{
    Log.Information("Starting Back-Dancer(BE) application");
    // Add services to the container.
    // https://www.claudiobernasconi.ch/2022/01/28/how-to-use-serilog-in-asp-net-core-web-api/ for precise logging
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger);

    // database connection // should be moved to persistance project
    builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddControllers();

    builder.Services
        .AddApplication()
        .AddInfrastructure()
        .AddPersistence()
        .AddPresentation();

    // for api versioning
    // https://christian-schou.dk/blog/how-to-use-api-versioning-in-net-core-web-api/
    builder.Services.AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                        new HeaderApiVersionReader("x-api-version"),
                                                        new MediaTypeApiVersionReader("x-api-version"));
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    // Add ApiExplorer to discover versions
    builder.Services.AddVersionedApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });
    //Swagger Documentation Section // probably need to remove this section
    var info = new OpenApiInfo()
    {
        Title = "Back dancer",
        Version = "v1",
        Description = "Description of your API",
        Contact = new OpenApiContact()
        {
            Name = "Your name",
            Email = "your@email.com",
        }

    };

    // read more about XML comments here
    // https://medium.com/@egwudaujenyuojo/implement-api-documentation-in-net-7-swagger-openapi-and-xml-comments-214caf53eece
    builder.Services.AddSwaggerGen(c =>
    {
        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

    builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

    // Add AutoMapper with a custom mapping profile
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // should be moved to other project
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    // https://levelup.gitconnected.com/two-different-approaches-for-global-exception-handling-in-asp-net-core-web-api-f815c27b1e2d
    builder.Services.AddTransient<ExceptionHandlingMiddleware>();
    var app = builder.Build();

    // for api versions
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(u =>
        {
            u.RouteTemplate = "swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}