using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Zones.Commands.UpdateZone;

public class UpdateZoneHandler(IAppDbContext dbContext) : IRequestHandler<UpdateZoneCommand, ZoneResult>
{
    public async Task<ZoneResult> Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Zone with ID {request.Id} not found.");

        var depot = await dbContext.Depots.FindAsync([request.DepotId], cancellationToken)
            ?? throw new InvalidOperationException($"Depot with ID {request.DepotId} not found.");

        zone.Name = request.Name;
        zone.DepotId = request.DepotId;
        zone.IsActive = request.IsActive;

        if (request.GeoJson != null)
        {
            zone.SetBoundaryFromGeoJson(request.GeoJson);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ZoneResult(zone.Id, zone.Name, zone.DepotId, zone.IsActive, zone.CreatedAt);
    }
}
