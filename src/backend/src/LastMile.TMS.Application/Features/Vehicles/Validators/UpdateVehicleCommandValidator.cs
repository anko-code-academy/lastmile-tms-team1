using FluentValidation;
using LastMile.TMS.Application.Features.Vehicles.Commands;

namespace LastMile.TMS.Application.Features.Vehicles.Validators;

public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vehicle ID is required");

        RuleFor(x => x.RegistrationPlate)
            .NotEmpty().WithMessage("Registration plate is required")
            .MaximumLength(20).WithMessage("Registration plate must not exceed 20 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid vehicle type");

        RuleFor(x => x.ParcelCapacity)
            .GreaterThan(0).WithMessage("Parcel capacity must be greater than 0");

        RuleFor(x => x.WeightCapacityKg)
            .GreaterThan(0).WithMessage("Weight capacity must be greater than 0");
    }
}
