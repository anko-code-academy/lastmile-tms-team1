using MediatR;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;
using LastMile.TMS.Application.Features.Users.Common;

namespace LastMile.TMS.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ITokenRevocationService tokenRevocationService)
    : IRequestHandler<DeactivateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        // Protect system admin
        SystemAdminProtection.ThrowIfSystemAdmin(user);

        if (user.Status == UserStatus.Inactive)
        {
            return Result<UserDto>.Failure("User is already inactive");
        }

        // Use domain method to set status
        user.Deactivate();

        // Immediate access revocation: LockoutEnd = DateTimeOffset.MaxValue, LockoutEnabled = true
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.LockoutEnabled = true;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Failed to deactivate user: {errors}");
        }

        // Revoke OpenIddict tokens and authorizations for the user
        await tokenRevocationService.RevokeUserTokensAsync(user.Id, cancellationToken);

        // Get role name
        string? roleName = null;
        if (user.RoleId.HasValue)
        {
            var role = await roleManager.FindByIdAsync(user.RoleId.Value.ToString());
            roleName = role?.Name;
        }

        return Result<UserDto>.Success(MapToDto(user, roleName));
    }

    private static UserDto MapToDto(User user, string? roleName)
    {
        return new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.PhoneNumber,
            user.Status,
            user.RoleId,
            roleName,
            user.ZoneId,
            user.Zone?.Name,
            user.DepotId,
            user.Depot?.Name,
            user.CreatedAt);
    }
}
