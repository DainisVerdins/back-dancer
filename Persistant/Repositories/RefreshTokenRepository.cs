using Application.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(DbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("refresh token can not be null or empty string");

        var output = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        return output;
    }
}
