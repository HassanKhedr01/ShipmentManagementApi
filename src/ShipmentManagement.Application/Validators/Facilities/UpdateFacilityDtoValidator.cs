using FluentValidation;
using ShipmentManagement.Application.DTOs.Facilities;

namespace ShipmentManagement.Application.Validators.Facilities;

public class UpdateFacilityDtoValidator : AbstractValidator<UpdateFacilityDto>
{
    public UpdateFacilityDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Facility name is required.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Facility address is required.");
        RuleFor(x => x.City).NotEmpty().WithMessage("Facility city is required.");
    }
}