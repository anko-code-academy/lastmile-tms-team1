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

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required.");

        RuleFor(x => x.Address.Street1)
            .NotEmpty().WithMessage("Street address is required.")
            .MaximumLength(500).WithMessage("Street address must not exceed 500 characters.");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(200).WithMessage("City must not exceed 200 characters.");

        RuleFor(x => x.Address.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(200).WithMessage("State must not exceed 200 characters.");

        RuleFor(x => x.Address.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.");

        RuleFor(x => x.Address.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .MaximumLength(2).WithMessage("Country code must be 2 characters.");
    }
}
