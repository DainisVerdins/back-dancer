using Application.Constants;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Models;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistance.Data;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly InitialUserSettings _initialUserSettings;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly DefaultOrganizationSettings _defaultOrganizationrSettings;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseInitializer(
        IUserService userService,
        IRoleService roleService,
        ILogger<DatabaseInitializer> logger,
        IOptions<InitialUserSettings> initialUserSettingsOptions,
        IOptions<DefaultOrganizationSettings> defaultOrganizationOptions,
        IUnitOfWork unitOfWork
        )
    {
        _roleService = roleService;
        _userService = userService;
        _logger = logger;
        _initialUserSettings = initialUserSettingsOptions.Value;
        _defaultOrganizationrSettings = defaultOrganizationOptions.Value;
        _unitOfWork = unitOfWork;
    }

    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedInitialUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in UserRole.GetRoleNames())
            if (!await _roleService.RoleExistsAsync(roleName))
                await _roleService.CreateRoleAsync(new Role { Name = roleName, RoleCode = roleName });
    }

    private async Task SeedInitialUsersAsync()
    {
        if (!await _userService.UserExistsAsync(_initialUserSettings.UserName))
        {
            var initialAdmin = new User
            {
                Email = _initialUserSettings.Email,
                UserName = _initialUserSettings.UserName,
                RequirePasswordChange = true
            };

            var result = await _userService.CreateUserAsync(initialAdmin, _initialUserSettings.Password);

            if (!result.Succeeded)
                throw new Exception($"Failed to init user: {string.Join(",",result.Errors.Select(e =>e.Description))}");

            await _unitOfWork.SaveChangesAsync();

            await _userService.AddRoleToUserByRoleNameAsync(initialAdmin, UserRole.SuperAdmin);
            await _userService.AddRoleToUserByRoleNameAsync(initialAdmin, UserRole.User);

        }
    }
}