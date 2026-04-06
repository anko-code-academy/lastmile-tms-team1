using FluentValidation;
using LastMile.TMS.Application.Features.Bins.Commands.UpdateBin;

namespace LastMile.TMS.Application.Features.Bins.Commands.UpdateBin;

public class UpdateBinCommandValidator : AbstractValidator<UpdateBinCommand>
{
    public UpdateBinCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Aisle)
            .GreaterThan(0).WithMessage("Aisle must be a positive number.");

        RuleFor(x => x.Slot)
            .GreaterThan(0).WithMessage("Slot must be a positive number.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be at least 1.")
            .LessThanOrEqualTo(10000).WithMessage("Capacity must not exceed 10000.");

        RuleFor(x => x.ZoneId)
            .NotEmpty().WithMessage("ZoneId is required.");
    }
}
