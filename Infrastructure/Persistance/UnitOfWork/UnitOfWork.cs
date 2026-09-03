using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Persistance.Data;
using Infrastructure.Persistance.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistance.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    public UnitOfWork(DataContext context)
    {
        _context = context;
        RefreshTokens = new RefreshTokenRepository(_context);
        Animals = new AnimalRepository(_context);
        AnimalImages = new AnimalImageRepository(_context);
        UserInvites = new UserInviteRepository(_context);
    }

    public IRefreshTokenRepository RefreshTokens { get; private set; }
    public IAnimalRepository Animals { get; private set; }
    public IAnimalImageRepository AnimalImages { get; private set; }
    public IUserInviteRepository UserInvites { get; private set; }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.Database.BeginTransactionAsync(
            cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
