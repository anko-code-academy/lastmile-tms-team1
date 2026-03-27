using FluentValidation;
using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

namespace LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

public class CreateZoneCommandValidator : AbstractValidator<CreateZoneCommand>
{
    public CreateZoneCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.GeoJson)
            .NotEmpty().WithMessage("GeoJson is required.");

        RuleFor(x => x.DepotId)
            .NotEmpty().WithMessage("DepotId is required.");
    }
}
