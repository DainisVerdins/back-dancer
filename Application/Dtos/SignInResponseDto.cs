namespace Application.Dtos;

public class SignInResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
}
