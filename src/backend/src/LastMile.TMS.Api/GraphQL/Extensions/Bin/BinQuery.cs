using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Bins.Queries;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainBin = LastMile.TMS.Domain.Entities.Bin;

namespace LastMile.TMS.Api.GraphQL.Extensions.Bin;

[ExtendObjectType(typeof(Query))]
public class BinQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DomainBin> GetBins([Service] AppDbContext context)
        => context.Bins.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<DomainBin> GetBin(Guid id, [Service] AppDbContext context)
        => context.Bins.AsNoTracking().Where(b => b.Id == id);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<IEnumerable<BinDto>> GetBinUtilizations(
        Guid zoneId,
        [Service] AppDbContext context,
        CancellationToken cancellationToken)
    {
        var binsQuery = context.Bins
            .AsNoTracking()
            .Include(b => b.Zone)
            .ThenInclude(z => z.Depot)
            .AsQueryable();

        if (zoneId != Guid.Empty)
        {
            binsQuery = binsQuery.Where(b => b.ZoneId == zoneId);
        }

        var bins = await binsQuery.ToListAsync(cancellationToken);

        var parcelCountsQuery = context.Parcels
            .AsNoTracking()
            .Where(p => p.Status == ParcelStatus.Sorted || p.Status == ParcelStatus.Staged);

        if (zoneId != Guid.Empty)
        {
            parcelCountsQuery = parcelCountsQuery.Where(p => p.ZoneId == zoneId);
        }

        var parcelCounts = await parcelCountsQuery
            .GroupBy(p => p.ZoneId)
            .Select(g => new { ZoneId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return bins.Select(b =>
        {
            var totalParcels = parcelCounts
                .FirstOrDefault(pc => pc.ZoneId == b.ZoneId)?.Count ?? 0;
            
            var utilization = b.Capacity > 0 ? (double)totalParcels / b.Capacity * 100 : 0;
            return new BinDto(
                b.Id,
                b.Label,
                b.Description,
                b.Aisle,
                b.Slot,
                b.Capacity,
                totalParcels,
                Math.Min(utilization, 100),
                b.IsActive,
                b.ZoneId,
                b.Zone?.Name ?? "Unknown",
                b.Zone?.Depot?.Name ?? "Unknown",
                b.CreatedAt);
        }).ToList();
    }
}
