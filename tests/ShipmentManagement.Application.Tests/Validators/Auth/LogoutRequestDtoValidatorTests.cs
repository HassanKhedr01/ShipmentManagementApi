using FluentAssertions;

using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Validators.Auth;

namespace ShipmentManagement.Application.Tests.Validators.Auth;

public class LogoutRequestDtoValidatorTests
{
    private readonly LogoutRequestDtoValidator _validator = new();
    
    [Fact]
    public void Validate_RefreshTokenIsValid_ShouldNotReturnError()
    {
        // Arrange
        var requestDto = new LogoutRequestDto { RefreshToken = "valid_refresh_token" };

        // Act
        var result = _validator.TestValidate(requestDto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }
    
    [Fact]
    public void Validate_RefreshTokenIsEmpty_ShouldReturnError()
    {
        // Arrange
        var requestDto = new LogoutRequestDto { RefreshToken = string.Empty };

        // Act
        var result = _validator.TestValidate(requestDto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}