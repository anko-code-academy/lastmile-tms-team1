using FluentValidation;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Application.Features.Routes.Validators;

public class AutoAssignParcelsByZoneCommandValidator : AbstractValidator<AutoAssignParcelsByZoneCommand>
{
    public AutoAssignParcelsByZoneCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty().WithMessage("Route ID is required");
    }
}
