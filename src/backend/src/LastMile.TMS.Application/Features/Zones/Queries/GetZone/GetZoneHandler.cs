using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;

namespace LastMile.TMS.Application.Features.Zones.Queries.GetZone;

public class GetZoneHandler(IAppDbContext dbContext) : IRequestHandler<GetZoneQuery, ZoneDto?>
{
    public async Task<ZoneDto?> Handle(GetZoneQuery request, CancellationToken cancellationToken)
    {
        var zone = await dbContext.Zones
            .Include(z => z.Depot)
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);

        if (zone == null) return null;

        var geoJson = new GeoJsonWriter().Write(zone.BoundaryGeometry);

        return new ZoneDto(
            zone.Id,
            zone.Name,
            geoJson,
            zone.DepotId,
            zone.Depot?.Name,
            zone.IsActive,
            zone.CreatedAt,
            zone.LastModifiedAt);
    }
}

public record ZoneDto(
    Guid Id,
    string Name,
    string GeoJson,
    Guid DepotId,
    string? DepotName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt);
