using Application.Interfaces.Repositories;

namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRefreshTokenRepository RefreshTokens { get; }
    IAnimalRepository Animals { get;  }
    IAnimalImageRepository AnimalImages { get;  }
    IUserInviteRepository UserInvites { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
