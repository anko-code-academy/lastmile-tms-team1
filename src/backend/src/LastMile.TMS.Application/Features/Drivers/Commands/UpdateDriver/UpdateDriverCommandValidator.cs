using FluentValidation;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;

public class UpdateDriverCommandValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Driver ID is required.");

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
            .MustAsync(async (cmd, email, cancellationToken) =>
                !await dbContext.Drivers.AnyAsync(d => d.Email == email && d.Id != cmd.Id && !d.IsDeleted, cancellationToken))
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

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MustAsync(async (cmd, email, cancellationToken) =>
            {
                var user = await dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
                if (user == null) return false;
                if (user.Role?.Name != Role.RoleNames.Driver) return false;
                if (await dbContext.Drivers.AnyAsync(d => d.UserId == user.Id && d.Id != cmd.Id && !d.IsDeleted, cancellationToken)) return false;
                return true;
            })
            .WithMessage("A user with this email must exist, have the Driver role, and not already have a driver entry.");
    }
}
