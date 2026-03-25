using MediatR;
using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Users.Commands.CompletePasswordReset;

public class CompletePasswordResetCommandHandler(UserManager<User> userManager)
    : IRequestHandler<CompletePasswordResetCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompletePasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists - return success for security
            return Result<bool>.Success(true);
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure($"Failed to reset password: {errors}");
        }

        return Result<bool>.Success(true);
    }
}
