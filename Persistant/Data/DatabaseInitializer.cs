using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Data;


// TODO IMPLEMENT DEFAULT INITIALIZER
// FIX THE ISSUE WITH MIGRATIONS AND UPDATE DATABASE
// MOVE SETTINGS TO APPSETINGS.JSON
// BETTER NAMING FOR USER ROLES MAYBE ADDING NEW CODE FOR ROLE
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
        IUnitOfWork unitOfWork
        )
    {
        _roleService = roleService;
        _userService = userService;
        _logger = logger;
        _initialUserSettings = initialUserSettingsOptions.Value;
        _unitOfWork = unitOfWork;
    }

    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedOrganizationRolesAsync();
        await SeedInitialUsersAsync();

        //  var defaultOrganization = new Organization { OrganizationName = _defaultOrganizationrSettings.OrganizationName };
        //   await SeedInitialOrganizationAsync(defaultOrganization);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in UserRole.GetRoleNames())
            if (!await _roleService.RoleExistsAsync(roleName))
                await _roleService.CreateRoleAsync(new Role { Name = roleName });
    }

    private async Task SeedInitialUsersAsync()
    {
        if (!await _userService.UserExistsAsync(_initialUserSettings.UserName))
        {
            var initialAdmin = new User
            {
                Email = _initialUserSettings.Email,
                UserName = _initialUserSettings.UserName
            };

            var isOk = await _userService.CreateUserAsync(initialAdmin, _initialUserSettings.Password);
            if (isOk)
                await _userService.AddRoleToUserByRoleNameAsync(initialAdmin, UserRole.Admin);
        }

        if (!await _userService.UserExistsAsync(_technicalUserSettings.UserName))
        {
            var technicalUser = new User
            {
                Email = _technicalUserSettings.Email,
                UserName = _technicalUserSettings.UserName
            };

            var isOk = await _userService.CreateUserAsync(technicalUser, _technicalUserSettings.Password);
            if (isOk)
                await _userService.AddRoleToUserByRoleNameAsync(technicalUser, UserRole.Technical);
        }
    }
}
