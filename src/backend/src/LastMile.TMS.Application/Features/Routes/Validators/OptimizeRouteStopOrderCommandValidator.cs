using FluentValidation;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Application.Features.Routes.Validators;

public class OptimizeRouteStopOrderCommandValidator : AbstractValidator<OptimizeRouteStopOrderCommand>
{
    public OptimizeRouteStopOrderCommandValidator()
    {
        RuleFor(x => x.RouteId).NotEmpty();
    }
}
