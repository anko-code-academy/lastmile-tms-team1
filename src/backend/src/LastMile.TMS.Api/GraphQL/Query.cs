// using HotChocolate;
// using HotChocolate.Authorization;
// using HotChocolate.Data;
// using LastMile.TMS.Domain.Entities;
// using LastMile.TMS.Persistence;
// using Microsoft.EntityFrameworkCore;

// namespace LastMile.TMS.Api.GraphQL;

// public class Query
// {
//     [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
//     [UsePaging(IncludeTotalCount = true)]
//     [UseProjection]
//     [UseFiltering]
//     [UseSorting]
//     public IQueryable<Depot> GetDepots([Service] AppDbContext context)
//         => context.Depots.AsNoTracking();

//     [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
//     [UseProjection]
//     [UseSingleOrDefault]
//     public IQueryable<Depot> GetDepot(Guid id, [Service] AppDbContext context)
//         => context.Depots.AsNoTracking().Where(d => d.Id == id);

//     [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
//     [UsePaging(IncludeTotalCount = true)]
//     [UseProjection]
//     [UseFiltering]
//     [UseSorting]
//     public IQueryable<Zone> GetZones([Service] AppDbContext context)
//         => context.Zones.AsNoTracking();

//     [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
//     [UseProjection]
//     [UseSingleOrDefault]
//     public IQueryable<Zone> GetZone(Guid id, [Service] AppDbContext context)
//         => context.Zones.AsNoTracking().Where(z => z.Id == id);
// }


using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL;

public class Query
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Depot> GetDepots([Service] AppDbContext context)
        => context.Depots.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseProjection]
    [UseSingleOrDefault]
    public IQueryable<Depot> GetDepot(Guid id, [Service] AppDbContext context)
        => context.Depots.AsNoTracking().Where(d => d.Id == id);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Zone> GetZones([Service] AppDbContext context)
        => context.Zones.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseProjection]
    [UseSingleOrDefault]
    public IQueryable<Zone> GetZone(Guid id, [Service] AppDbContext context)
        => context.Zones.AsNoTracking().Where(z => z.Id == id);
}
