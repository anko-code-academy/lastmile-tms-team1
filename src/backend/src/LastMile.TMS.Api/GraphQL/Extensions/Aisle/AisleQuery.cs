using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainAisle = LastMile.TMS.Domain.Entities.Aisle;

namespace LastMile.TMS.Api.GraphQL.Extensions.Aisle;

[ExtendObjectType(typeof(Query))]
public class AisleQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainAisle> GetAisles([Service] AppDbContext context)
        => context.Aisles.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainAisle> GetAisle(Guid id, [Service] AppDbContext context)
        => context.Aisles.AsNoTracking().Where(a => a.Id == id);
}
