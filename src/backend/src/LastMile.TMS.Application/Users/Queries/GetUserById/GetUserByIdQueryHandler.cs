using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

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
            roleName,
            user.RoleId,
            user.ZoneId,
            user.DepotId,
            user.CreatedAt);
    }
}
