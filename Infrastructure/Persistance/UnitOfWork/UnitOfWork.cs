using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Persistance.Data;
using Infrastructure.Persistance.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistance.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    private IDbContextTransaction? _transaction;
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


    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException(
                "Transaction has not been started.");

        await _transaction.CommitAsync(cancellationToken);

        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(cancellationToken);

        await _transaction.DisposeAsync();
        _transaction = null;
    }

}
