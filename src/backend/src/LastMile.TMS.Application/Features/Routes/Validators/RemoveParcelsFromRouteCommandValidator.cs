using FluentValidation;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Application.Features.Routes.Validators;

public class RemoveParcelsFromRouteCommandValidator : AbstractValidator<RemoveParcelsFromRouteCommand>
{
    public RemoveParcelsFromRouteCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty().WithMessage("Route ID is required");

        RuleFor(x => x.ParcelIds)
            .NotEmpty().WithMessage("At least one parcel ID is required");
    }
}
