using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistance.Data;

namespace Infrastructure.Persistance.Repositories;

public class WeatherForecastRepository : GenericRepository<WeatherForecast>, IWeatherForecastRepository
{
    private static readonly string[] Summaries = [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
    public WeatherForecastRepository(DataContext context) : base(context)
    {
    }
    public IEnumerable<WeatherForecast> GetPopularDevelopers(int count)
    {
        var generatedForeCasts = new List<WeatherForecast>();
        for (int i = 0; i < count; i++)
        {
            generatedForeCasts.Add(new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });
        }
        return generatedForeCasts;
    }
}