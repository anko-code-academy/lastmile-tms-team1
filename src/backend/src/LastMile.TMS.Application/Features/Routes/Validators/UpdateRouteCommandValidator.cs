using FluentValidation;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Application.Features.Routes.Validators;

public class UpdateRouteCommandValidator : AbstractValidator<UpdateRouteCommand>
{
    public UpdateRouteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Route ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Route name is required")
            .MaximumLength(100).WithMessage("Route name must not exceed 100 characters");

        RuleFor(x => x.PlannedStartTime)
            .NotEmpty().WithMessage("Planned start time is required")
            .Must(time => time > DateTime.UtcNow).WithMessage("Planned start time must be in the future");

        RuleFor(x => x.TotalDistanceKm)
            .GreaterThan(0).WithMessage("Total distance must be greater than 0");

        RuleFor(x => x.TotalParcelCount)
            .GreaterThanOrEqualTo(0).WithMessage("Total parcel count must be non-negative");
    }
}
