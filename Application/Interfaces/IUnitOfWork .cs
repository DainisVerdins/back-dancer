using Application.Interfaces.Repositories;

namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRefreshTokenRepository RefreshTokens { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
