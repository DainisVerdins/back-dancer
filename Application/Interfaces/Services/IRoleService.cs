using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IRoleService
{
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<IList<Role>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string roleName);
    Task<IList<Role>> GetRolesForUserAsync(User user);
    Task<bool> IsUserInRoleAsync(User user, string roleName);
    Task CreateRoleAsync(Role roleToCreate);
}
