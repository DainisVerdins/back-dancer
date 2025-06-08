using Domain.Models;
using System.Security.Claims;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<bool> UserExistsAsync(string userName);
    Task<User?> GetUserByUserNameAsync(string userName);
    Task<bool> CreateUserAsync(User userToCreate, string password);
    Task<IList<Claim>> GetUserClaims(User user);
    Task AddRoleToUserAsync(User user, string roleName);
    Task<IList<Claim>> GetClaimsForAccessTokenByUserIdAsync(Guid userId);
    Task<User?> GetCurrentUserAsync();
}
