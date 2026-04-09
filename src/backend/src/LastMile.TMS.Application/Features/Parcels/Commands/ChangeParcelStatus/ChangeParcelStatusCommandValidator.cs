using FluentValidation;

namespace LastMile.TMS.Application.Features.Parcels.Commands.ChangeParcelStatus;

public class ChangeParcelStatusCommandValidator : AbstractValidator<ChangeParcelStatusCommand>
{
    public ChangeParcelStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid parcel status.");

        RuleFor(x => x.ExceptionReason)
            .NotNull().WithMessage("Exception reason is required when status is Exception.")
            .When(x => x.NewStatus == Domain.Enums.ParcelStatus.Exception);
    }
}
