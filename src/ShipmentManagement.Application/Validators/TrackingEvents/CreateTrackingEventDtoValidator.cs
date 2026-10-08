using FluentValidation;

using ShipmentManagement.Application.DTOs.TrackingEvents;

namespace ShipmentManagement.Application.Validators.TrackingEvents;

public class CreateTrackingEventDtoValidator : AbstractValidator<CreateTrackingEventDto>
{
    public CreateTrackingEventDtoValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid new status.");
        RuleFor(x => x.Description).MaximumLength(256).WithMessage("Description must not exceed 256 characters.");
    }
}