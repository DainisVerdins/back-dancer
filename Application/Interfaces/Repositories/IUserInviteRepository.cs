using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IUserInviteRepository : IGenericRepository<UserInvite>
{
    Task<UserInvite?> GetPendingByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
