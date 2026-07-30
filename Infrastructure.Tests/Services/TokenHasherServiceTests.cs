using AwesomeAssertions;
using Infrastructure.Services;

namespace Infrastructure.Tests.Services;

public class TokenHasherServiceTests
{
    private readonly TokenHasherService _service;

    public TokenHasherServiceTests()
    {
        _service = new TokenHasherService();
    }

    [Fact]
    public void Hash_ShouldReturnSha256Hash_WhenInputIsValid()
    {
        // Arrange
        const string input = "HelloWorld";

        // Act
        var result = _service.Hash(input);

        // Assert
        result.Should().Be("872E4E50CE9990D8B041330C47C9DDD11BEC6B503AE9386A99DA8584E9BB12C4");
    }

    [Fact]
    public void Hash_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        // Act
        Action action = () => _service.Hash(null!);

        // Assert
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("textToHash");
    }

    [Fact]
    public void Hash_ShouldThrowArgumentNullException_WhenInputIsEmpty()
    {
        // Act
        Action action = () => _service.Hash(string.Empty);

        // Assert
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("textToHash");
    }
}
