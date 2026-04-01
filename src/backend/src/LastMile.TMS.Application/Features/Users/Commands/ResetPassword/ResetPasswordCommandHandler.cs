using MediatR;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Features.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    UserManager<User> userManager,
    IEmailService emailService)
    : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists - always return success for security
            return Result<bool>.Success(true);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailService.SendPasswordResetTokenAsync(user.Email!, token, cancellationToken);

        // Always return true - don't reveal user existence
        return Result<bool>.Success(true);
    }
}
