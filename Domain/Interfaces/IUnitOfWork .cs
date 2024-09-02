
namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWeatherForecastRepository WeatherForecasts { get; }
    int Complete();
}
