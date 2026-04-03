using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainParcel = LastMile.TMS.Domain.Entities.Parcel;
using DomainParcelAuditLog = LastMile.TMS.Domain.Entities.ParcelAuditLog;

namespace LastMile.TMS.Api.GraphQL.Extensions.Parcel;

[ExtendObjectType(typeof(Query))]
public class ParcelQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.WarehouseOperator, Role.RoleNames.Dispatcher })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainParcel> GetParcels([Service] AppDbContext context)
        => context.Parcels.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.WarehouseOperator, Role.RoleNames.Dispatcher })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainParcel> GetParcel(Guid id, [Service] AppDbContext context)
        => context.Parcels.AsNoTracking().Where(p => p.Id == id);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainParcelAuditLog> GetParcelAuditLogs(Guid parcelId, [Service] AppDbContext context)
        => context.ParcelAuditLogs.AsNoTracking().Where(al => al.ParcelId == parcelId);
}
