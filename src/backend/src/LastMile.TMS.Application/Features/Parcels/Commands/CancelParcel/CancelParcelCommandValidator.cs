using FluentValidation;

namespace LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;

public class CancelParcelCommandValidator : AbstractValidator<CancelParcelCommand>
{
    public CancelParcelCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
    }
}
