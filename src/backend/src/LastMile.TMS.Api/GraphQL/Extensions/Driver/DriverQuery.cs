using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainDriver = LastMile.TMS.Domain.Entities.Driver;

namespace LastMile.TMS.Api.GraphQL.Extensions.Driver;

[ExtendObjectType(typeof(Query))]
public class DriverQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainDriver> GetDrivers([Service] AppDbContext context)
        => context.Drivers.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseProjection]
    [UseSingleOrDefault]
    public IQueryable<DomainDriver> GetDriver(Guid id, [Service] AppDbContext context)
        => context.Drivers
            .Include(d => d.Zone)
            .Include(d => d.Depot)
            .AsNoTracking()
            .Where(d => d.Id == id);
}
