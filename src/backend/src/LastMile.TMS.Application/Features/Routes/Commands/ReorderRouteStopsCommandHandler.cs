using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class ReorderRouteStopsCommandHandler(IAppDbContext context) : IRequestHandler<ReorderRouteStopsCommand, RouteDto>
{
    public async Task<RouteDto> Handle(ReorderRouteStopsCommand request, CancellationToken cancellationToken)
    {
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
            throw new InvalidOperationException("Stops can only be reordered on routes in Draft status.");
        }

        var stopIdsSet = request.StopIdsInOrder.ToHashSet();
        var routeStopIds = route.RouteStops.Select(s => s.Id).ToHashSet();

        // Verify all provided stop IDs belong to this route
        if (!stopIdsSet.SetEquals(routeStopIds))
        {
            throw new InvalidOperationException("Provided stop IDs do not match the route's stops.");
        }

        // Update sequence numbers
        for (var i = 0; i < request.StopIdsInOrder.Count; i++)
        {
            var stop = route.RouteStops.First(s => s.Id == request.StopIdsInOrder[i]);
            stop.SequenceNumber = i + 1;
        }

        var depotGeo = await context.Zones
            .Where(z => z.Id == route.ZoneId)
            .Select(z => z.Depot.Address.GeoLocation)
            .FirstOrDefaultAsync(cancellationToken);
        route.RecalculateDistance(depotGeo);

        await context.SaveChangesAsync(cancellationToken);

        return route.ToDto();
    }
}
