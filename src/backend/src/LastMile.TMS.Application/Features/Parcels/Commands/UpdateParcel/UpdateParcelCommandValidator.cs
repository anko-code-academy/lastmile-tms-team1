using FluentValidation;

namespace LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;

public class UpdateParcelCommandValidator : AbstractValidator<UpdateParcelCommand>
{
    public UpdateParcelCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.")
            .When(x => x.Length.HasValue);

        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.")
            .When(x => x.Width.HasValue);

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.")
            .When(x => x.Height.HasValue);
    }
}
