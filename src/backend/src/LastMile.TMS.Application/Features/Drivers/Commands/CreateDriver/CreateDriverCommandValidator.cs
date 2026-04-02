using FluentValidation;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

public class CreateDriverCommandValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MustAsync(async (email, cancellationToken) =>
            {
                var user = await dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
                if (user == null) return false;
                if (user.Role?.Name != Role.RoleNames.Driver) return false;
                if (await dbContext.Drivers.AnyAsync(d => d.UserId == user.Id && !d.IsDeleted, cancellationToken)) return false;
                return true;
            })
            .WithMessage("A user with this email must exist, have the Driver role, and not already have a driver entry.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required.")
            .MaximumLength(50).WithMessage("License number must not exceed 50 characters.");

        RuleFor(x => x.LicenseExpiryDate)
            .NotEmpty().WithMessage("License expiry date is required.")
            .Must(date => date > DateTimeOffset.UtcNow).WithMessage("License expiry date must be in the future.");
    }
}
