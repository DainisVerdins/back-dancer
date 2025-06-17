using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Models;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistance.Data;


// TODO IMPLEMENT DEFAULT INITIALIZER
// MOVE SETTINGS TO APPSETINGS.JSON
// BETTER NAMING FOR USER ROLES MAYBE ADDING NEW CODE FOR ROLE
public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly InitialUserSettings _initialUserSettings;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public DatabaseInitializer(
        IUserService userService,
        IRoleService roleService,
        ILogger<DatabaseInitializer> logger,
        IOptions<InitialUserSettings> initialUserSettingsOptions
        )
    {
        _roleService = roleService;
        _userService = userService;
        _logger = logger;
        _initialUserSettings = initialUserSettingsOptions.Value;
    }

    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedInitialUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in UserRoleName.GetRoleNames())
            if (!await _roleService.RoleExistsAsync(roleName))
                await _roleService.CreateRoleAsync(new Role { Name = roleName });
    }

    private async Task SeedInitialUsersAsync()
    {
        if (await _userService.UserExistsAsync(_initialUserSettings.UserName))
            return;

        var initialAdmin = new User
        {
            Email = _initialUserSettings.Email,
            UserName = _initialUserSettings.Email
        };

        var isOk = await _userService.CreateUserAsync(initialAdmin, _initialUserSettings.Password);
        if (isOk)
            await _userService.AddRoleToUserByRoleNameAsync(initialAdmin, UserRoleName.Admin);

    }
}
