using Application.Constants;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Infrastructure.Services;

public class UserService : IUserService
{

    private readonly UserManager<User> _userManager;
    private readonly IRoleService _roleService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserService(
        UserManager<User> userManager,
        IRoleService roleService, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleService = roleService;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> CreateUserAsync(User userToCreate, string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        if (await _userManager.FindByNameAsync(userToCreate?.UserName ?? "") != null)
            return true;


        if (userToCreate is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));
        var result = await _userManager.CreateAsync(userToCreate, password);

        return result.Succeeded == true;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByEmailAsync(email);

        return appUser;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        if (userId < 1)
            throw new ArgumentException("id value cannot be less than 1", nameof(userId));

        var appUser = await _userManager.FindByIdAsync(userId.ToString());

        return appUser;
    }

    public async Task<User?> GetUserByUserNameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<IList<Claim>> GetUserClaims(User user)
    {
        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        return await _userManager.GetClaimsAsync(user);
    }

    public async Task<bool> UserExistsAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByNameAsync(userName);

        return appUser != null;
    }

    public async Task AddRoleToUserAsync(User user, string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var doesRoleExist = await _roleService.RoleExistsAsync(roleName);
        if (!doesRoleExist)
            throw new Exception($"Role with name {roleName} does not exit!");

        var appUser = await _userManager.FindByNameAsync(user.UserName ?? "");

        if (appUser is null)
            return;

        await _userManager.AddToRoleAsync(appUser, roleName);
    }

    public async Task<IList<Claim>> GetClaimsForAccessTokenByUserIdAsync(int userId, string? roleName = null)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            throw new Exception(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        var outputClaims = new List<Claim>() {
            new(CustomClaimType.UserId, userId.ToString()),
            new(CustomClaimType.UserName, user.UserName ?? user.Email ?? ""),
            new(ClaimTypes.Name, user.UserName?? user.Email ?? ""),
            new(ClaimTypes.Email, user.Email ?? "")
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        if (userClaims != null)
            outputClaims.AddRange(userClaims);

        if (user.RequirePasswordChange)
            outputClaims.Add(new Claim(CustomClaimType.ForceChangePassword, "true"));

        string? roleToEmbed = roleName;

        if (string.IsNullOrEmpty(roleToEmbed))
        {
            var userRoles = await _roleService.GetRolesForUserAsync(user);

            if (userRoles != null && userRoles.Count == 1)
                roleToEmbed = userRoles.First().RoleCode;
        }

        if (!string.IsNullOrEmpty(roleToEmbed))
        {
            outputClaims.AddRange(new List<Claim>
        {
            new(CustomClaimType.RoleName, roleToEmbed),
            new(ClaimTypes.Role, roleToEmbed)
        });
        }

        return outputClaims;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null)
            return null;

        var userIdClaim = principal.FindFirst(CustomClaimType.UserId)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<IdentityResult> AddRoleToUserByRoleNameAsync(User user, string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var doesRoleExist = await _roleService.RoleExistsAsync(roleName);
        if (!doesRoleExist)
            throw new Exception($"Role with name {roleName} does not exit!");

        return await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<bool> IsLockedOutAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        return await _userManager.IsLockedOutAsync(appUser);
    }

    public async Task IncrementAccessFailedCountAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        await _userManager.AccessFailedAsync(appUser);
    }

    public async Task ResetAccessFailedCountAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
            throw new ArgumentNullException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        await _userManager.ResetAccessFailedCountAsync(appUser);
    }


    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user), ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());

        if (appUser is null)
            throw new Exception(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        return await _userManager.GeneratePasswordResetTokenAsync(appUser);
    }
    public async Task UpdateUserAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user), ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
            throw new Exception(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        await _userManager.UpdateAsync(appUser);
    }
    public async Task<bool> ResetPasswordAsync(User user, string resetToken, string newPassword)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user), ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        if (string.IsNullOrEmpty(resetToken))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty), nameof(resetToken));

        if (string.IsNullOrEmpty(newPassword))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty), nameof(newPassword));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
            throw new Exception(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        var result = await _userManager.ResetPasswordAsync(appUser, resetToken, newPassword);

        return result.Succeeded;
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user), ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty));

        if (string.IsNullOrEmpty(oldPassword))
            throw new ArgumentException(ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty), nameof(oldPassword));

        if (string.IsNullOrEmpty(newPassword))
            throw new ArgumentException(ErrorMessages.GetArgumentMessage(ArgumentErrorCode.ArgumentIsEmpty), nameof(newPassword));

        if (string.Equals(oldPassword, newPassword))
            throw new Exception("new and old password are same");

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (result.Succeeded && user.RequirePasswordChange)
        {
            user.RequirePasswordChange = false;
            await _userManager.UpdateAsync(user);
        }

        return result;
    }

    public async Task<bool> IsInRoleAsync(User user, string roleName)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user), ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty), nameof(roleName));

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
            throw new Exception(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        return await _userManager.IsInRoleAsync(appUser, roleName);
    }
}
