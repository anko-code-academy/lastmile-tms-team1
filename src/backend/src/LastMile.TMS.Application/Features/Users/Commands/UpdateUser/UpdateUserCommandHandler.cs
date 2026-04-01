using MediatR;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Application.Features.Users.Validation;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Features.Users.Common;

namespace LastMile.TMS.Application.Features.Users.Commands.UpdateUser;

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

        // Check if phone number already exists for another user (if provided)
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var existingByPhone = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone && u.Id != user.Id, cancellationToken);
            if (existingByPhone != null)
            {
                return Result<UserDto>.Failure("User with this phone number already exists");
            }
        }

        // Update role if provided
        if (request.RoleId.HasValue)
        {
            var role = await roleManager.FindByIdAsync(request.RoleId.Value.ToString());
            if (role == null)
            {
                return Result<UserDto>.Failure("Role not found");
            }
            user.RoleId = request.RoleId.Value;
            user.RoleName = role.Name;

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
            var zone = await context.Zones.FindAsync(new object[] { request.ZoneId.Value }, cancellationToken);
            user.ZoneId = request.ZoneId;
            user.ZoneName = zone?.Name;
        }
        else
        {
            user.ZoneId = null;
            user.ZoneName = null;
        }

        if (request.DepotId.HasValue)
        {
            var depot = await context.Depots.FindAsync(new object[] { request.DepotId.Value }, cancellationToken);
            user.DepotId = request.DepotId;
            user.DepotName = depot?.Name;
        }
        else
        {
            user.DepotId = null;
            user.DepotName = null;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Failed to update user: {errors}");
        }

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.PhoneNumber,
            user.Status,
            user.RoleName,
            user.RoleId,
            user.ZoneId,
            user.ZoneName,
            user.DepotId,
            user.DepotName,
            user.CreatedAt));
    }
}
