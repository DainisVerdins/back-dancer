using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Authentication;

public class SignInViewModel
{
    [Required]
    public string Email { get; init; } = string.Empty;
    [Required]
    public string Password { get; init; } = string.Empty;
}
