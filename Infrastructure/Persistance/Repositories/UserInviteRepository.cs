using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class UserInviteRepository : GenericRepository<UserInvite>, IUserInviteRepository
{
    public UserInviteRepository(DbContext context) : base(context)
    {
    }

    public async Task<UserInvite?> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));

        return await _dbSet
            .FirstOrDefaultAsync(
                x =>
                    x.Email == email &&
                    x.Status == InviteStatus.Pending &&
                    x.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);
    }

    public async Task<UserInvite?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _dbSet
        .FirstOrDefaultAsync(
            x => x.Token == tokenHash,
            cancellationToken);
    }
}
