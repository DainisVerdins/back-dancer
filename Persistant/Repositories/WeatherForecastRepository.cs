using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Repository;

namespace Persistence.Repositories;

public class WeatherForecastRepository : GenericRepository<WeatherForecast>, IWeatherForecastRepository
{
    public WeatherForecastRepository(DataContext context) : base(context)
    {
    }
    public IEnumerable<WeatherForecast> GetPopularDevelopers(int count)
    {
        return _context.WeatherForecasts.OrderByDescending(d => d.Date).Take(count).ToList();
    }
}