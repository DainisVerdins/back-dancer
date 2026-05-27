using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Infrastructure.Tests.Fakes;

public class FakeUserManager : UserManager<User>
{
    private readonly IList<IPasswordValidator<User>> _passwordValidators;

    public FakeUserManager(IList<IPasswordValidator<User>> passwordValidators)
        : base(
            Mock.Of<IUserStore<User>>(),
            null!, null!,null!,
            passwordValidators,
            null!, null!, null!, null!)
    {
        _passwordValidators = passwordValidators;
    }
}
