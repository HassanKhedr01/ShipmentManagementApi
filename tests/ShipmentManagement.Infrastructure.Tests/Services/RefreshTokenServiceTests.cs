using FluentAssertions;

using ShipmentManagement.Infrastructure.Services;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Infrastructure.Tests.Services;

public class RefreshTokenServiceTests
{
    [Theory, AutoMoqData]
    public void GenerateRefreshToken_ShouldReturnUniqueAndCorrectlySizedString(RefreshTokenService service)
    {
        // Act
        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
        token1.Should().HaveLength(88); // 64 bytes in base64 is 88 characters

        var act = () => Convert.FromBase64String(token1);
        act.Should().NotThrow(because: "the generated token should be a valid base64 string");
    }

    [Theory, AutoMoqData]
    public void HashRefreshToken_ValidRefreshToken_ReturnsExpectedDeterministicHash(RefreshTokenService service)
    {
        // Arrange
        const string inputToken = "Secure_Test_Token_123";
        // Pre-calculated SHA256 hash of "Secure_Test_Token_123" converted to Base64
        
        const string expectedHash = "8v+eVetTcX7JrKiVhzVP0ZaWO4Mb64nLLHCpa0i8IR4=";         
        // Act
        var actualHash = service.HashRefreshToken(inputToken);
        
        // Assert
        actualHash.Should().Be(expectedHash);
        actualHash.Should().HaveLength(44); // 32 bytes in base64 is 44 characters 
    }
    
    [Theory, AutoMoqData]
    public void HashRefreshToken_EmptyInput_ReturnsExpectedDeterministicHash(RefreshTokenService service)
    {
        // Arrange
        var inputToken = string.Empty;
        // Pre-calculated SHA256 hash of an empty string converted to Base64
        const string expectedHash = "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=";
        
        // Act
        var actualHash = service.HashRefreshToken(inputToken);
        
        // Assert
        actualHash.Should().Be(expectedHash);
        actualHash.Should().HaveLength(44); // 32 bytes in base64 is 44 characters 
    }
}