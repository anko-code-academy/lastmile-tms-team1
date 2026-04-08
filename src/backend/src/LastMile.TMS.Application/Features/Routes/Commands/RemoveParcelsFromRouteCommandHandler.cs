using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class RemoveParcelsFromRouteCommandHandler(IAppDbContext context) : IRequestHandler<RemoveParcelsFromRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(RemoveParcelsFromRouteCommand request, CancellationToken cancellationToken)
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
            throw new InvalidOperationException("Parcels can only be removed from routes in Draft status.");
        }

        var parcels = await context.Parcels
            .Where(p => request.ParcelIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        // Clear RouteStopId on each parcel
        foreach (var parcel in parcels)
        {
            parcel.RouteStopId = null;
        }

        // Remove stops that now have zero parcels
        var emptyStops = route.RouteStops.Where(s => !s.Parcels.Any(p => !request.ParcelIds.Contains(p.Id))).ToList();
        foreach (var stop in emptyStops)
        {
            context.RouteStops.Remove(stop);
        }

        // Re-sequence remaining stops
        var remainingStops = route.RouteStops
            .Where(s => !emptyStops.Contains(s))
            .OrderBy(s => s.SequenceNumber)
            .ToList();

        for (var i = 0; i < remainingStops.Count; i++)
        {
            remainingStops[i].SequenceNumber = i + 1;
        }

        route.RecalculateTotals();
        await context.SaveChangesAsync(cancellationToken);

        return route.ToDto();
    }
}
