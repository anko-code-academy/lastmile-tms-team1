using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using DomainParcel = LastMile.TMS.Domain.Entities.Parcel;
using DomainParcelAuditLog = LastMile.TMS.Domain.Entities.ParcelAuditLog;

namespace LastMile.TMS.Api.GraphQL.Extensions.Parcel;

[ExtendObjectType(typeof(Query))]
public class ParcelQuery
{
    [Authorize]
    [UsePaging(IncludeTotalCount = true, MaxPageSize = 100)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainParcel> GetParcels(
        string? recipientSearch,
        string? addressSearch,
        [Service] AppDbContext context)
    {
        var query = context.Parcels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(recipientSearch))
        {
            query = query.Where(p =>
                EF.Property<NpgsqlTsVector>(p.RecipientAddress, "RecipientNameSearchVector")
                    .Matches(recipientSearch));
        }

        if (!string.IsNullOrWhiteSpace(addressSearch))
        {
            query = query.Where(p =>
                EF.Property<NpgsqlTsVector>(p.RecipientAddress, "AddressSearchVector")
                    .Matches(addressSearch));
        }

        return query;
    }

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

    [Authorize(Roles = new[] { Role.RoleNames.WarehouseOperator, Role.RoleNames.Admin })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainParcel> GetParcelByTrackingNumber(string trackingNumber, [Service] AppDbContext context)
        => context.Parcels.AsNoTracking().Where(p => p.TrackingNumber == trackingNumber);

    [Authorize]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<TrackingEvent> GetTrackingEvents(
        Guid parcelId, [Service] AppDbContext context)
        => context.TrackingEvents.AsNoTracking()
            .Where(te => te.ParcelId == parcelId)
            .OrderByDescending(te => te.Timestamp);
}
