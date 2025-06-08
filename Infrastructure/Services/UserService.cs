using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Persistence.Identity.Constants;
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
            throw new ArgumentException("id value can not default guid value");

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

    public async Task<IList<Claim>> GetClaimsForAccessTokenByUserIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            throw new Exception(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        var outputClaims = new List<Claim>() {
            new(CustomClaimType.UserId, userId.ToString())
        };

        var userClaims = await _userManager.GetClaimsAsync(user);

        if (userClaims != null)
            outputClaims.AddRange(userClaims);

        outputClaims.AddRange(
            new Claim(CustomClaimType.UserName, user.UserName ?? user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? "")
        );

        var userRoles = await _roleService.GetRolesForUserAsync(user);

        if (userRoles != null && userRoles.Count > 0)
            foreach (var userRole in userRoles)
            {
                outputClaims.AddRange(
                    new Claim(CustomClaimType.RoleName, userRole.Name ?? "user"),
                    new Claim(ClaimTypes.Role, userRole.Name ?? "user")
                );
            }

        return outputClaims;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null)
            return null;

        var userIdClaim = principal.FindFirst(CustomClaimType.UserId)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return await _userManager.FindByIdAsync(userId.ToString());
    }
}
