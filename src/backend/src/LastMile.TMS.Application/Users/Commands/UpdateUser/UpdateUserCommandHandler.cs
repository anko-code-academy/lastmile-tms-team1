using MediatR;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Application.Users.Validation;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;
using LastMile.TMS.Application.Users.Common;

namespace LastMile.TMS.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IAppDbContext context)
    : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        // Protect system admin
        SystemAdminProtection.ThrowIfSystemAdmin(user);

        // Update basic properties using domain method
        user.UpdateName(request.FirstName, request.LastName);
        user.UpdateEmail(request.Email);
        user.UpdatePhone(request.Phone);

        // Update role if provided
        string? roleName = null;
        if (request.RoleId.HasValue)
        {
            var role = await roleManager.FindByIdAsync(request.RoleId.Value.ToString());
            if (role == null)
            {
                return Result<UserDto>.Failure("Role not found");
            }
            user.AssignRole(request.RoleId.Value);
            roleName = role.Name;

            // Update in identity role table too
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await userManager.AddToRoleAsync(user, role.Name!);
        }

        // Validate depot/zone assignments
        var assignmentError = await UserManagementRules.EnsureValidAssignmentsAsync(
            context, request.DepotId, request.ZoneId);
        if (assignmentError != null)
        {
            return Result<UserDto>.Failure(assignmentError);
        }

        // Update zone/depot assignment
        if (request.ZoneId.HasValue)
        {
            user.AssignToZone(request.ZoneId.Value);
        }
        else if (request.DepotId.HasValue)
        {
            user.AssignToDepot(request.DepotId.Value);
        }
        else
        {
            // Clear both using domain method
            user.ClearZoneAndDepot();
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Failed to update user: {errors}");
        }

        // Get role name if not already fetched
        if (roleName == null && user.RoleId.HasValue)
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
            roleName,
            user.RoleId,
            user.ZoneId,
            user.DepotId,
            user.CreatedAt);
    }
}
