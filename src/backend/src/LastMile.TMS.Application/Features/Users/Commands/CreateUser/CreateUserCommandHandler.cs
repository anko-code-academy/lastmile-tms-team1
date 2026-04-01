using MediatR;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Application.Features.Users.Validation;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IAppDbContext context)
    : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<UserDto>.Failure("User with this email already exists");
        }

        // Check if username already exists
        var existingByUserName = await userManager.FindByNameAsync(request.Email.ToLowerInvariant());
        if (existingByUserName != null)
        {
            return Result<UserDto>.Failure("User with this username already exists");
        }

        // Check if phone number already exists (if provided)
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var existingByPhone = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone, cancellationToken);
            if (existingByPhone != null)
            {
                return Result<UserDto>.Failure("User with this phone number already exists");
            }
        }

        // Check if role exists
        var role = await roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null)
        {
            return Result<UserDto>.Failure("Role not found");
        }

        // Validate depot/zone assignments
        var assignmentError = await UserManagementRules.EnsureValidAssignmentsAsync(
            context, request.DepotId, request.ZoneId);
        if (assignmentError != null)
        {
            return Result<UserDto>.Failure(assignmentError);
        }

        // Create user using factory method
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            phone: request.Phone);

        // Assign role directly
        user.RoleId = request.RoleId;
        user.RoleName = role.Name;

        // Assign zone or depot if provided
        if (request.ZoneId.HasValue)
        {
            var zone = await context.Zones.FindAsync(new object[] { request.ZoneId.Value }, cancellationToken);
            user.ZoneId = request.ZoneId;
            user.ZoneName = zone?.Name;
        }

        if (request.DepotId.HasValue)
        {
            var depot = await context.Depots.FindAsync(new object[] { request.DepotId.Value }, cancellationToken);
            user.DepotId = request.DepotId;
            user.DepotName = depot?.Name;
        }

        // Create user with password
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Failed to create user: {errors}");
        }

        // Add user to role
        var roleResult = await userManager.AddToRoleAsync(user, role.Name!);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Failed to assign role: {errors}");
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
