using FluentValidation;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Application.Features.Routes.Validators;

public class CreateRouteCommandValidator : AbstractValidator<CreateRouteCommand>
{
    public CreateRouteCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Route name is required")
            .MaximumLength(100).WithMessage("Route name must not exceed 100 characters");

        RuleFor(x => x.PlannedStartTime)
            .NotEmpty().WithMessage("Planned start time is required");

        RuleFor(x => x.TotalDistanceKm)
            .GreaterThan(0).WithMessage("Total distance must be greater than 0");

        RuleFor(x => x.TotalParcelCount)
            .GreaterThanOrEqualTo(0).WithMessage("Total parcel count must be non-negative");
    }
}
