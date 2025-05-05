using Application.Entities;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private const int REFRESH_TOKEN_LENGTH = 64;
    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public TokenResponse GetRefreshToken()
    {
        var randomNumber = new byte[REFRESH_TOKEN_LENGTH];

        using var generator = RandomNumberGenerator.Create();

        generator.GetBytes(randomNumber);

        return new TokenResponse
        {
            Token = Convert.ToBase64String(randomNumber),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays),
        };
    }

    public TokenResponse GenerateAccessToken(IEnumerable<Claim> claims)
    {
        if (claims is null)
            throw new ArgumentNullException(nameof(claims), ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty));

        var claimsToAdd = new List<Claim> { new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        claimsToAdd.AddRange(claims);

        var tokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes);
        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claimsToAdd,
            expires: tokenExpires,
            signingCredentials: new SigningCredentials(_jwtSettings.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

        return new TokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpiresAt = tokenExpires
        };
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException(ErrorMessages.GetMessage(ErrorCode.ArgumentIsEmpty), nameof(token));

        var validation = new TokenValidationParameters
        {
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = _jwtSettings.GetSymmetricSecurityKey(),
            ValidateLifetime = false
        };

        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }
}
