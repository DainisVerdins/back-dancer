using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<bool> UserExistsAsync(string userName);
    Task<User?> GetUserByUserNameAsync(string userName);
    Task<bool> CreateUserAsync(User userToCreate, string password);
    Task<IList<Claim>> GetUserClaims(User user);
    Task<IdentityResult> AddRoleToUserByRoleNameAsync(User user, string roleName);
    Task<IList<Claim>> GetClaimsForAccessTokenByUserIdAsync(int userId, string? roleName = null);
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsLockedOutAsync(User user);
    Task IncrementAccessFailedCountAsync(User user);
    Task ResetAccessFailedCountAsync(User user);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> ResetPasswordAsync(User user, string token, string newPassword);
    Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);
    Task<bool> IsInRoleAsync(User user, string roleName);
}
