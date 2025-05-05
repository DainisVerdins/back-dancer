using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Persistance.Repositories;
using Persistence.Data;
using Persistence.Repositories;

namespace Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    public UnitOfWork(DataContext context)
    {
        _context = context;
        RefreshTokens = new RefreshTokenRepository(_context);
        WeatherForecasts = new WeatherForecastRepository(_context);
    }

    public IRefreshTokenRepository RefreshTokens { get; private set; }
    public IWeatherForecastRepository WeatherForecasts { get; private set; }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
