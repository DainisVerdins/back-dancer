namespace Application.ViewModels.User;

public class SendInviteViewModel
{
    public required string Email { get; init; }

    public required List<string> Roles { get; init; }
}
