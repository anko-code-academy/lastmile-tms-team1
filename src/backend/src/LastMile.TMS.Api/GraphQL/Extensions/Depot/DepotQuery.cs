using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainDepot = LastMile.TMS.Domain.Entities.Depot;

namespace LastMile.TMS.Api.GraphQL.Extensions.Depot;

[ExtendObjectType(typeof(Query))]
public class DepotQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainDepot> GetDepots([Service] AppDbContext context)
        => context.Depots.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainDepot> GetDepot(Guid id, [Service] AppDbContext context)
        => context.Depots.AsNoTracking().Where(d => d.Id == id);
}
