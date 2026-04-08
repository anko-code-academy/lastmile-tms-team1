using FluentValidation;
using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;

public class UpdateDriverCommandValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Driver ID is required.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required.")
            .MaximumLength(50).WithMessage("License number must not exceed 50 characters.");

        RuleFor(x => x.LicenseExpiryDate)
            .NotEmpty().WithMessage("License expiry date is required.")
            .Must(date => date > DateTimeOffset.UtcNow).WithMessage("License expiry date must be in the future.");
    }
}
