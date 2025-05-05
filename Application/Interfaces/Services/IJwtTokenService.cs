using Application.Entities;
using System.Security.Claims;

namespace Application.Interfaces.Services;

public interface IJwtTokenService
{
    TokenResponse GenerateAccessToken(IEnumerable<Claim> claimsToAdd);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
    TokenResponse GetRefreshToken();
}
