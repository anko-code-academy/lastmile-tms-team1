using System.Linq.Expressions;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.GraphQL.Extensions.UserManagement;

public static class UserMappings
{
    public static Expression<Func<User, UserDto>> ToUserDto => u => new UserDto(
        u.Id,
        u.FirstName,
        u.LastName,
        u.Email!,
        u.PhoneNumber,
        u.Status,
        u.Role != null ? u.Role.Name : null,
        u.RoleId,
        u.ZoneId,
        u.Zone != null ? u.Zone.Name : null,
        u.DepotId,
        u.Depot != null ? u.Depot.Name : null,
        u.CreatedAt);
}
