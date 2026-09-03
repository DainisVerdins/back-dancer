using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IUserInviteRepository : IGenericRepository<UserInvite>
{
    Task<UserInvite?> GetPendingByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
    Task<UserInvite?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
