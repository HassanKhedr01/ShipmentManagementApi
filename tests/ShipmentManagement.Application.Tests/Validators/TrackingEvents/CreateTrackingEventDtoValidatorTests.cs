using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.TrackingEvents;
using ShipmentManagement.Application.Validators.TrackingEvents;

namespace ShipmentManagement.Application.Tests.Validators.TrackingEvents;

public class CreateTrackingEventDtoValidatorTests
{
    private readonly CreateTrackingEventDtoValidator _validator = new();

    [Theory]
    [InlineData(DTOs.Common.Status.Created)]
    [InlineData(DTOs.Common.Status.PickedUp)]
    [InlineData(DTOs.Common.Status.InTransit)]
    [InlineData(DTOs.Common.Status.ArrivedAtFacility)]
    [InlineData(DTOs.Common.Status.OutForDelivery)]
    [InlineData(DTOs.Common.Status.Delivered)]
    [InlineData(DTOs.Common.Status.Delayed)]
    [InlineData(DTOs.Common.Status.Cancelled)]
    public void NewStatus_ValidNewStatus_ShouldNotHaveValidationError(DTOs.Common.Status status)
    {
        // Arrange
        var dto = new CreateTrackingEventDto { NewStatus = status };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewStatus);
    }

    [Fact]
    public void NewStatus_InvalidNewStatus_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTrackingEventDto { NewStatus = (DTOs.Common.Status)(999) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewStatus).WithErrorMessage("Invalid new status.");
    }
    
    [Fact]
    public void Description_ValidDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreateTrackingEventDto { Description = "Valid description" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
    
    [Fact]
    public void Description_TooLongDescription_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTrackingEventDto { Description = new string('a', 257) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage("Description must not exceed 256 characters.");
    }
    
    [Fact]
    public void Description_NullDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new CreateTrackingEventDto { Description = null };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}