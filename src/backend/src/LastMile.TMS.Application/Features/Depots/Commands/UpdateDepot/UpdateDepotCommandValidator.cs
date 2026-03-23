using FluentValidation;
using LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;

namespace LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;

public class UpdateDepotCommandValidator : AbstractValidator<UpdateDepotCommand>
{
    public UpdateDepotCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
