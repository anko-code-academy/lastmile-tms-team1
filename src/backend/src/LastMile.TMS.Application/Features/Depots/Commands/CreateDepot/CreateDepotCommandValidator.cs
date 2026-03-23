using FluentValidation;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

namespace LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

public class CreateDepotCommandValidator : AbstractValidator<CreateDepotCommand>
{
    public CreateDepotCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
