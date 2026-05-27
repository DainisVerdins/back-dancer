using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Infrastructure.Tests.Fakes;

public static class IdentityMockFactory
{
    public static Mock<UserManager<User>> CreateUserManager(
        IUserStore<User>? store = null,
        IList<IPasswordValidator<User>>? passwordValidators = null)
    {
        var userStore = store ?? new Mock<IUserStore<User>>().Object;

        // If no validators are passed, use an empty list instead of null!
        var validators = passwordValidators ?? new List<IPasswordValidator<User>>();

        return new Mock<UserManager<User>>(
            userStore,
            null!, // IOptions<IdentityOptions>
            null!, // IPasswordHasher<User>
            null!, // IEnumerable<IUserValidator<User>>
            validators, // Passed straight to the base class constructor!
            null!, // ILookupNormalizer
            null!, // IdentityErrorDescriber
            null!, // IServiceProvider
            null!  // ILogger
        );
    }

    public static Mock<RoleManager<Role>> CreateRoleManager(IRoleStore<Role>? store = null)
    {
        var roleStore = store ?? new Mock<IRoleStore<Role>>().Object;
        return new Mock<RoleManager<Role>>(
            roleStore, null!, null!, null!, null!);
    }
}
