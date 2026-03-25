using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types.Pagination;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Application.Users.Queries.GetUserById;
using LastMile.TMS.Application.Users.Queries.GetUsers;
using LastMile.TMS.Application.Users.Queries.GetUserManagementLookups;
using LastMile.TMS.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class UserManagementQuery
{
    [UsePaging(DefaultPageSize = 50)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<UserDto> GetUsers(
        [Service] AppDbContext context)
    {
        return context.Users
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .Select(u => new UserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email!,
                u.PhoneNumber,
                u.Status,
                u.Role != null ? u.Role.Name : null,
                u.RoleId,
                u.ZoneId,
                u.DepotId,
                u.CreatedAt));
    }

    public async Task<UserDto?> GetUserById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), ct);
        return result.Value;
    }

    public async Task<UserManagementLookupsDto> GetUserManagementLookups(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserManagementLookupsQuery(), ct);
        return result.Value!;
    }
}
