using MediatR;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;
using LastMile.TMS.Application.Features.Users.Common;

namespace LastMile.TMS.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
    : IRequestHandler<ActivateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        // Protect system admin
        SystemAdminProtection.ThrowIfSystemAdmin(user);

        if (user.Status == UserStatus.Active)
        {
            return Result<UserDto>.Failure("User is already active");
        }

        // Use domain method to set status
        user.Activate();

        // Remove lockout to allow login
        user.LockoutEnd = null;
        user.LockoutEnabled = false;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Failed to activate user: {errors}");
        }

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