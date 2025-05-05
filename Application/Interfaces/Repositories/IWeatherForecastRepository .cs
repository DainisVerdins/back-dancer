using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IWeatherForecastRepository : IGenericRepository<WeatherForecast>
{
    IEnumerable<WeatherForecast> GetPopularDevelopers(int count);
}