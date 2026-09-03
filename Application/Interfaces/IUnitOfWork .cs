using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRefreshTokenRepository RefreshTokens { get; }
    IAnimalRepository Animals { get; }
    IAnimalImageRepository AnimalImages { get; }
    IUserInviteRepository UserInvites { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}
