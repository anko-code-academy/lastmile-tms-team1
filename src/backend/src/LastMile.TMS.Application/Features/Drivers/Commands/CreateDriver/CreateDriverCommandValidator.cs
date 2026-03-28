using FluentValidation;
using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

public class CreateDriverCommandValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MustAsync(async (email, cancellationToken) =>
                !await dbContext.Drivers.AnyAsync(d => d.Email == email && !d.IsDeleted, cancellationToken))
            .WithMessage("A driver with this email already exists.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required.")
            .MaximumLength(50).WithMessage("License number must not exceed 50 characters.");

        RuleFor(x => x.LicenseExpiryDate)
            .NotEmpty().WithMessage("License expiry date is required.")
            .Must(date => date > DateTimeOffset.UtcNow).WithMessage("License expiry date must be in the future.");

        RuleFor(x => x.ZoneId)
            .NotEmpty().WithMessage("Zone is required.")
            .MustAsync(async (zoneId, cancellationToken) =>
                await dbContext.Zones.AnyAsync(z => z.Id == zoneId && !z.IsDeleted, cancellationToken))
            .WithMessage("Zone must exist.");

        RuleFor(x => x.DepotId)
            .NotEmpty().WithMessage("Depot is required.")
            .MustAsync(async (depotId, cancellationToken) =>
                await dbContext.Depots.AnyAsync(d => d.Id == depotId && !d.IsDeleted, cancellationToken))
            .WithMessage("Depot must exist.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User is required.");
    }
}
