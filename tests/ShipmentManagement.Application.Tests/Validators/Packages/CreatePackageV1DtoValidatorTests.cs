using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.DTOs.Packages;
using ShipmentManagement.Application.Validators.Common;
using ShipmentManagement.Application.Validators.Packages;

namespace ShipmentManagement.Application.Tests.Validators.Packages;

public class CreatePackageV1DtoValidatorTests
{
    private readonly CreatePackageV1DtoValidator _validator;

    public CreatePackageV1DtoValidatorTests()
    {
        var addressValidator = new AddressDtoValidator();
        _validator = new CreatePackageV1DtoValidator(addressValidator);
    }

    #region Name

    [Fact]
    public void Name_InvalidName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { Name = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Package name is required.");
    }

    [Fact]
    public void Name_ValidName_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { Name = "Valid Package Name" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region SenderName

    [Fact]
    public void SenderName_InvalidSenderName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { SenderName = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SenderName)
            .WithErrorMessage("Sender name is required.");
    }

    [Fact]
    public void SenderName_ValidSenderName_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { SenderName = "Valid Sender Name" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SenderName);
    }

    #endregion

    #region RecipientName

    [Fact]
    public void RecipientName_InvalidRecipientName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { RecipientName = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecipientName)
            .WithErrorMessage("Recipient name is required.");
    }

    [Fact]
    public void RecipientName_ValidRecipientName_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { RecipientName = "Valid Recipient Name" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecipientName);
    }

    #endregion

    #region OriginAddress

    [Fact]
    public void OriginAddress_InvalidOriginAddress_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { OriginAddress = new AddressDto() };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginAddress.City);
        result.ShouldHaveValidationErrorFor(x => x.OriginAddress.Street);
        result.ShouldHaveValidationErrorFor(x => x.OriginAddress.PostalCode);
    }

    [Fact]
    public void OriginAddress_ValidOriginAddress_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto
        {
            OriginAddress = new AddressDto { City = "Valid City", Street = "Valid Street", PostalCode = "12345" }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OriginAddress);
    }

    #endregion

    #region DestinationAddress

    [Fact]
    public void DestinationAddress_InvalidDestinationAddress_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto { DestinationAddress = new AddressDto() };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DestinationAddress.City);
        result.ShouldHaveValidationErrorFor(x => x.DestinationAddress.Street);
        result.ShouldHaveValidationErrorFor(x => x.DestinationAddress.PostalCode);
    }

    [Fact]
    public void DestinationAddress_ValidDestinationAddress_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreatePackageV1Dto
        {
            DestinationAddress =
                new AddressDto { City = "Valid City", Street = "Valid Street", PostalCode = "12345" }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DestinationAddress);
    }

    #endregion
}