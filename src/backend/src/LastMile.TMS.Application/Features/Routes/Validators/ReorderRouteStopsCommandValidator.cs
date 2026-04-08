using FluentValidation;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Application.Features.Routes.Validators;

public class ReorderRouteStopsCommandValidator : AbstractValidator<ReorderRouteStopsCommand>
{
    public ReorderRouteStopsCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty().WithMessage("Route ID is required");

        RuleFor(x => x.StopIdsInOrder)
            .NotEmpty().WithMessage("At least one stop ID is required");

        RuleFor(x => x.StopIdsInOrder)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Stop IDs must not contain duplicates");
    }
}
