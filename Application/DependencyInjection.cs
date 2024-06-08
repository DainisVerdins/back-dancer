using Application.MappingProfiles;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        // TODO:  need to figure out, if mapping should be here or in presentation layer
        services.AddAutoMapper(new[] { typeof(WeatherForecastMappingProfile).Assembly });

        return services;
    }
}
