using FluentValidation.TestHelper;

using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Application.Validators.Facilities;

namespace ShipmentManagement.Application.Tests.Validators.Facilities;

public class CreateFacilityDtoValidatorTests
{
    private readonly CreateFacilityDtoValidator _validator = new();

    [Fact]
    public void Name_ValidName_ShouldNotHaveValidationError()
    {
        var dto = new CreateFacilityDto { Name = "Valid Facility Name" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Name_InvalidName_ShouldHaveValidationError()
    {
        var dto = new CreateFacilityDto { Name = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Facility name is required.");
    }
    
    [Fact]
    public void Address_ValidAddress_ShouldNotHaveValidationError()
    {
        var dto = new CreateFacilityDto { Address = "Valid Facility Address" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }
    
    [Fact]
    public void Address_InvalidAddress_ShouldHaveValidationError()
    {
        var dto = new CreateFacilityDto { Address = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Address).WithErrorMessage("Facility address is required.");
    }
    
    [Fact]
    public void City_ValidCity_ShouldNotHaveValidationError()
    {
        var dto = new CreateFacilityDto { City = "Valid Facility City" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }
    
    [Fact]
    public void City_InvalidCity_ShouldHaveValidationError()
    {
        var dto = new CreateFacilityDto { City = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.City).WithErrorMessage("Facility city is required.");
    }

}