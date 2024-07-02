using Domain.Interfaces;
using Persistence.Data;
using Persistence.Repositories;

namespace Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    public UnitOfWork(DataContext context)
    {
        _context = context;
        WeatherForecasts = new WeatherForecastRepository(_context);
    }
    public IWeatherForecastRepository WeatherForecasts { get; private set; }
    public int Complete()
    {
        return _context.SaveChanges();
    }
    public void Dispose()
    {
        _context.Dispose();
    }
}