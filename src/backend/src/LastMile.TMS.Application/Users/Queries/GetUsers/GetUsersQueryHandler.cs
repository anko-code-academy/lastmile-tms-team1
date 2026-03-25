using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
    : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
{
    public async Task<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // UserManager.Users.ToList() is the standard pattern for Identity - no async equivalent exists
        var users = userManager.Users.ToList();

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            if (user.IsDeleted) continue;

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                var role = await roleManager.FindByIdAsync(user.RoleId.Value.ToString());
                roleName = role?.Name;
            }

            userDtos.Add(MapToDto(user, roleName));
        }

        return Result<List<UserDto>>.Success(userDtos);
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
