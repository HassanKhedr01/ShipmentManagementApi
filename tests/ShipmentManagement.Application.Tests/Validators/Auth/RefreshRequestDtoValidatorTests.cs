using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Validators.Auth;

namespace ShipmentManagement.Application.Tests.Validators.Auth;

public class RefreshRequestDtoValidatorTests
{
    private readonly RefreshRequestDtoValidator _validator = new();

    [Fact]
    public void ExpiredAccessToken_EmptyAccessToken_HasValidationError()
    {
        // Arrange
        var request = new RefreshRequestDto { ExpiredAccessToken = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiredAccessToken)
            .WithErrorMessage("Expired access token is required.");
    }

    [Fact]
    public void ExpiredAccessToken_ValidAccessToken_HasNoValidationError()
    {
        // Arrange
        var request = new RefreshRequestDto { ExpiredAccessToken = "expired-access-token" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiredAccessToken);
    }

    [Fact]
    public void RefreshToken_EmptyRefreshToken_HasValidationError()
    {
        // Arrange
        var request = new RefreshRequestDto { RefreshToken = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage("Refresh token is required.");
    }

    [Fact]
    public void RefreshToken_ValidRefreshToken_HasNoValidationError()
    {
        // Arrange
        var request = new RefreshRequestDto { RefreshToken = "valid-refresh-token" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void Validate_ValidRequestDto_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new RefreshRequestDto
        {
            ExpiredAccessToken = "expired-access-token", RefreshToken = "valid-refresh-token"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}