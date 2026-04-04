using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainParcel = LastMile.TMS.Domain.Entities.Parcel;

namespace LastMile.TMS.Api.GraphQL.Extensions.Parcel;

[ExtendObjectType(typeof(Query))]
public class ParcelQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.WarehouseOperator, Role.RoleNames.Admin })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainParcel> GetParcels([Service] AppDbContext context)
        => context.Parcels.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.WarehouseOperator, Role.RoleNames.Admin })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainParcel> GetParcel(Guid id, [Service] AppDbContext context)
        => context.Parcels.AsNoTracking().Where(d => d.Id == id);
}
