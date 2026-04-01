using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Extensions.UserManagement;

[ExtendObjectType(typeof(Query))]
public class UserManagementQuery
{
    [Authorize(Roles = [Role.RoleNames.Admin])]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context)
        => context.Users.AsNoTracking();

    [Authorize(Roles = [Role.RoleNames.Admin])]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> GetUser(Guid id, [Service] AppDbContext context)
        => context.Users.AsNoTracking().Where(u => u.Id == id);

    [Authorize(Roles = [Role.RoleNames.Admin])]
    public async Task<UserManagementLookupsDto> GetUserManagementLookups(
        [Service] AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        var roles = await context.Roles
            .Select(r => new RoleLookupDto(r.Id, r.Name!, r.Description))
            .ToListAsync(cancellationToken);

        var depots = await context.Depots
            .Where(d => d.IsActive)
            .Select(d => new DepotLookupDto(d.Id, d.Name))
            .ToListAsync(cancellationToken);

        var zones = await context.Zones
            .Where(z => z.IsActive)
            .Select(z => new ZoneLookupDto(z.Id, z.Name, z.DepotId))
            .ToListAsync(cancellationToken);

        return new UserManagementLookupsDto(roles, depots, zones);
    }
}
