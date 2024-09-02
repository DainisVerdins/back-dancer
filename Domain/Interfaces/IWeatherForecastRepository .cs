using Domain.Models;

namespace Domain.Interfaces;

public interface IWeatherForecastRepository : IGenericRepository<WeatherForecast>
{
    IEnumerable<WeatherForecast> GetPopularDevelopers(int count);
}