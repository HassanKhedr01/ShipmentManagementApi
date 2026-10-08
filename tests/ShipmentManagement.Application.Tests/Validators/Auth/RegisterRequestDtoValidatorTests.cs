using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Validators.Auth;

namespace ShipmentManagement.Application.Tests.Validators.Auth;

public class RegisterRequestDtoValidatorTests
{
    private readonly RegisterRequestDtoValidator _validator = new();

    [Fact]
    public void UserName_EmptyUserName_HasValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { UserName = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username is required.");
    }

    [Fact]
    public void UserName_ValidValue_HasNoValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { UserName = "testuser" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Email_EmptyEmail_HasValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { Email = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("invalid@")]
    public void Email_InvalidValue_HasValidationError(string email)
    {
        // Arrange
        var request = new RegisterRequestDto { Email = email };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email address.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@example.com")]
    [InlineData("valid@example")]
    public void Email_ValidValue_HasNoValidationError(string email)
    {
        // Arrange
        var request = new RegisterRequestDto { Email = email };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Password_EmptyPassword_HasValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { Password = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Password_LessThanSixCharacters_HasValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { Password = "12345" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters long.");
    }

    [Fact]
    public void Password_SixCharacters_HasNoValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { Password = "123456" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ConfirmPassword_EmptyConfirmPassword_HasValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { ConfirmPassword = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Confirm Password is required.");
    }

    [Fact]
    public void ConfirmPassword_DifferentFromPassword_HasValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { Password = "password123", ConfirmPassword = "different" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match.");
    }

    [Fact]
    public void ConfirmPassword_MatchesPassword_HasNoValidationError()
    {
        // Arrange
        var request = new RegisterRequestDto { Password = "password123", ConfirmPassword = "password123" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}