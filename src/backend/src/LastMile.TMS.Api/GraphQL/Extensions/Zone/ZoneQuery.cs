using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainZone = LastMile.TMS.Domain.Entities.Zone;

namespace LastMile.TMS.Api.GraphQL.Extensions.Zone;

[ExtendObjectType(typeof(Query))]
public class ZoneQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainZone> GetZones([Service] AppDbContext context)
        => context.Zones.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainZone> GetZone(Guid id, [Service] AppDbContext context)
        => context.Zones.AsNoTracking().Where(z => z.Id == id);
}
