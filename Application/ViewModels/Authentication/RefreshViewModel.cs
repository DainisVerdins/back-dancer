namespace Application.ViewModels.Authentication;

public class RefreshViewModel
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
