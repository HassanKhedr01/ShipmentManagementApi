using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.Validators.Common;

namespace ShipmentManagement.Application.Tests.Validators.Common;

public class AddressDtoValidatorTests
{
    private readonly AddressDtoValidator _validator = new();
    
    [Fact]
    public void Address_ValidAddress_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new AddressDto { Street = "123 Main St", City = "Anytown", PostalCode = "12345" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Street);
        result.ShouldNotHaveValidationErrorFor(x => x.City);
        result.ShouldNotHaveValidationErrorFor(x => x.PostalCode);
    }
    
    [Fact]
    public void Address_InvalidAddress_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new AddressDto { Street = "", City = "", PostalCode = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street).WithErrorMessage("Street is required.");
        result.ShouldHaveValidationErrorFor(x => x.City).WithErrorMessage("City is required.");
        result.ShouldHaveValidationErrorFor(x => x.PostalCode).WithErrorMessage("Postal code is required.");
    }
    
    [Fact]
    public void Address_TooLongAddress_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new AddressDto
        {
            Street = new string('A', 101),
            City = new string('B', 101),
            PostalCode = new string('C', 21)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street).WithErrorMessage("Street must not exceed 100 characters.");
        result.ShouldHaveValidationErrorFor(x => x.City).WithErrorMessage("City must not exceed 100 characters.");
        result.ShouldHaveValidationErrorFor(x => x.PostalCode).WithErrorMessage("Postal code must not exceed 20 characters.");
    }
}