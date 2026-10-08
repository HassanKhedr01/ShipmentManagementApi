using FluentValidation;
using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.DTOs.Packages;

namespace ShipmentManagement.Application.Validators.Packages;

public class CreatePackageV2DtoValidator : AbstractValidator<CreatePackageV2Dto>
{
    public CreatePackageV2DtoValidator(IValidator<AddressDto> addressValidator)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Package name is required.");
        RuleFor(x => x.SenderName).NotEmpty().WithMessage("Sender name is required.");
        RuleFor(x => x.RecipientName).NotEmpty().WithMessage("Recipient name is required.");
        RuleFor(x => x.OriginAddress).NotNull().WithMessage("Origin address is required.")
            .SetValidator(addressValidator);
        RuleFor(x => x.DestinationAddress).NotNull().WithMessage("Destination address is required.")
            .SetValidator(addressValidator);
        RuleFor(x => x.DeliveryType).IsInEnum()
            .WithMessage("Invalid delivery type.");
    }
}