using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Application.Features.Routes.Services;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class OptimizeRouteStopOrderCommandHandler(
    IAppDbContext context,
    IRouteStopOptimizer optimizer) : IRequestHandler<OptimizeRouteStopOrderCommand, RouteDto>
{
    public async Task<RouteDto> Handle(OptimizeRouteStopOrderCommand request, CancellationToken cancellationToken)
    {
        // Load route with stops — avoid deep Zone.Depot.Address include to prevent circular dependency
        var route = await context.Routes
            .Include(r => r.RouteStops).ThenInclude(s => s.Parcels)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .Include(r => r.Zone)
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.RouteId} not found.");
        }

        if (route.Status != RouteStatus.Draft)
        {
            throw new InvalidOperationException("Stops can only be optimized on routes in Draft status.");
        }

        if (route.RouteStops.Count < 2)
        {
            return route.ToDto();
        }

        // Resolve depot coordinates via separate query to avoid circular dependency
        Point? depotGeo = null;
        double depotLat = 0, depotLon = 0;
        if (route.ZoneId.HasValue)
        {
            depotGeo = await context.Zones
                .Where(z => z.Id == route.ZoneId.Value)
                .Select(z => z.Depot.Address.GeoLocation)
                .FirstOrDefaultAsync(cancellationToken);

            if (depotGeo is not null)
            {
                depotLat = depotGeo.Y;
                depotLon = depotGeo.X;
            }
        }

        // Build geo info list
        var geoStops = route.RouteStops
            .Select((s, i) => new RouteStopGeoInfo(
                s.Id,
                s.GeoLocation?.Y ?? double.NaN,
                s.GeoLocation?.X ?? double.NaN,
                i))
            .ToList();

        var optimizedIds = optimizer.OptimizeStopOrder(geoStops, depotLat, depotLon);

        // Update sequence numbers in two phases to avoid unique index collision on (RouteId, SequenceNumber):
        // Phase 1: Clear all sequence numbers to negative values
        foreach (var stop in route.RouteStops)
        {
            stop.SequenceNumber = -stop.SequenceNumber - 1000;
        }
        await context.SaveChangesAsync(cancellationToken);

        // Phase 2: Assign final optimized sequence numbers
        for (var i = 0; i < optimizedIds.Count; i++)
        {
            var stop = route.RouteStops.First(s => s.Id == optimizedIds[i]);
            stop.SequenceNumber = i + 1;
        }

        // Recalculate distance with optimized stop order
        route.RecalculateDistance(depotGeo);

        await context.SaveChangesAsync(cancellationToken);

        return route.ToDto();
    }
}
