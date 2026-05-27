using AwesomeAssertions;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Key = "super_secret_test_key_that_is_long_enough_for_hmac_256_algorithm",
            Issuer = "http://localhost:5117",
            Audience = "http://localhost:5173",
            AccessTokenLifetimeMinutes = 20,
            RefreshTokenLifetimeDays = 2
        };

        var options = new Mock<IOptions<JwtSettings>>();
        options.Setup(x => x.Value).Returns(_jwtSettings);

        _sut = new JwtTokenService(options.Object);
    }

    #region GetRefreshToken tests

    [Fact]
    public void GetRefreshToken_ShouldReturnNonEmptyToken()
    {
        // Act
        var result = _sut.GetRefreshToken();

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var result = _sut.GetRefreshToken();

        // Assert
        var act = () => Convert.FromBase64String(result.Token);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetRefreshToken_ShouldReturnCorrectExpiry()
    {
        // Act
        var before = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays).AddSeconds(-1);
        var result = _sut.GetRefreshToken();
        var after = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays).AddSeconds(1);

        // Assert
        result.ExpiresAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void GetRefreshToken_CalledTwice_ShouldReturnDifferentTokens()
    {
        // Act
        var first = _sut.GetRefreshToken();
        var second = _sut.GetRefreshToken();

        // Assert
        first.Token.Should().NotBe(second.Token);
    }
    #endregion

    #region GenerateAccessToken tests
    [Fact]
    public void GenerateAccessToken_WithValidClaims_ShouldReturnNonEmptyToken()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Email, "test@example.com")
        };

        // Act
        var result = _sut.GenerateAccessToken(claims);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_WithValidClaims_ShouldReturnCorrectExpiry()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };

        // Act
        var before = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes).AddSeconds(-1);
        var result = _sut.GenerateAccessToken(claims);
        var after = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes).AddSeconds(1);

        // Assert
        result.ExpiresAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };

        // Act
        var result = _sut.GenerateAccessToken(claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(result.Token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectIssuerAndAudience()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };

        // Act
        var result = _sut.GenerateAccessToken(claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Issuer.Should().Be(_jwtSettings.Issuer);
        jwt.Audiences.Should().Contain(_jwtSettings.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainPassedClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Email, "test@example.com")
        };

        // Act
        var result = _sut.GenerateAccessToken(claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Claims.Should().Contain(c => c.Value == "testuser");
        jwt.Claims.Should().Contain(c => c.Value == "test@example.com");
    }

    [Fact]
    public void GenerateAccessToken_WithNullClaims_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _sut.GenerateAccessToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region GetPrincipalFromToken tests
    [Fact]
    public void GetPrincipalFromToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };
        var token = _sut.GenerateAccessToken(claims);

        // Act
        var principal = _sut.GetPrincipalFromToken(token.Token);

        // Assert
        principal.Should().NotBeNull();
    }

    [Fact]
    public void GetPrincipalFromToken_WithValidToken_ShouldContainClaims()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };
        var token = _sut.GenerateAccessToken(claims);

        // Act
        var principal = _sut.GetPrincipalFromToken(token.Token);

        // Assert
        principal!.Claims.Should().Contain(c => c.Value == "testuser");
    }

    [Fact]
    public void GetPrincipalFromToken_WithNullToken_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _sut.GetPrincipalFromToken(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPrincipalFromToken_WithEmptyToken_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _sut.GetPrincipalFromToken(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPrincipalFromToken_WithInvalidToken_ShouldThrowException()
    {
        // Act
        var act = () => _sut.GetPrincipalFromToken("invalid.token.here");

        // Assert
        act.Should().Throw<Exception>();
    }
    #endregion
}
