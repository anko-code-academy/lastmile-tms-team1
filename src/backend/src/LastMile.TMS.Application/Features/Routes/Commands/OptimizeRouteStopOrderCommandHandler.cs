using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Application.Features.Routes.Services;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Domain.Extensions;
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
        var route = await context.Routes
            .Include(r => r.RouteStops).ThenInclude(s => s.Parcels)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .Include(r => r.Zone).ThenInclude(z => z.Depot).ThenInclude(d => d.Address)
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

        // Resolve depot coordinates
        var depotGeo = route.Zone?.Depot?.Address?.GeoLocation;

        // Build geo info list
        var stops = route.RouteStops
            .Select((s, i) => new RouteStopGeoInfo(
                s.Id,
                s.GeoLocation?.Y ?? double.NaN,
                s.GeoLocation?.X ?? double.NaN,
                i))
            .ToList();

        var optimizedIds = optimizer.OptimizeStopOrder(
            stops,
            depotGeo?.Y ?? 0,
            depotGeo?.X ?? 0);

        // Update sequence numbers
        for (var i = 0; i < optimizedIds.Count; i++)
        {
            var stop = route.RouteStops.First(s => s.Id == optimizedIds[i]);
            stop.SequenceNumber = i + 1;
        }

        await context.SaveChangesAsync(cancellationToken);

        return route.ToDto();
    }
}
