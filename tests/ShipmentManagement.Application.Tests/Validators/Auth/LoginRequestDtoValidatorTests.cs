using FluentAssertions;
using FluentValidation.TestHelper;
using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Validators.Auth;

namespace ShipmentManagement.Application.Tests.Validators.Auth;

public class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _validator = new();

    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new LoginRequestDto { Email = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public void Email_WhenInvalid_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = new LoginRequestDto { Email = email };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email address.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@example.com")]
    [InlineData("valid@example")]
    public void Email_WhenValid_ShouldNotHaveValidationError(string email)
    {
        // Arrange
        var request = new LoginRequestDto { Email = email };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Password_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new LoginRequestDto { Password = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Password_WhenProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new LoginRequestDto { Password = "password123" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validator_ValidLoginRequestProvided_ShouldBeValid()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}