using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly UserManager<User> _userManager;

    public PasswordService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    public async Task<IList<string>> ValidatePasswordAsync(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var errors = new List<string>();

        // Loop through all registered password validators
        foreach (var validator in _userManager.PasswordValidators)
        {
            var result = await validator.ValidateAsync(_userManager, new User(), password);
            if (!result.Succeeded)
                errors.AddRange(result.Errors.Select(e => e.Description));
        }

        return errors;
    }
}
