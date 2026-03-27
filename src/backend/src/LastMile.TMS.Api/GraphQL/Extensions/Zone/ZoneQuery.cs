using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainZone = LastMile.TMS.Domain.Entities.Zone;
using LastMile.TMS.Domain.Entities;

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
    [UseProjection]
    [UseFirstOrDefault]
    public IQueryable<DomainZone> GetZone(Guid id, [Service] AppDbContext context)
        => context.Zones.Include(z => z.Depot).AsNoTracking().Where(z => z.Id == id);
}