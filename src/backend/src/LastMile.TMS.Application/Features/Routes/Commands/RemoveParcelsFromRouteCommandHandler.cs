using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

using ParcelStatus = LastMile.TMS.Domain.Enums.ParcelStatus;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class RemoveParcelsFromRouteCommandHandler(
    IAppDbContext context,
    IBinAssignmentService binAssignmentService,
    ICurrentUserService currentUserService) : IRequestHandler<RemoveParcelsFromRouteCommand, RouteDto>
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

        // Clear RouteStopId on each parcel and transition back to Sorted
        var removedParcelIds = request.ParcelIds.ToHashSet();
        foreach (var parcel in parcels)
        {
            parcel.RouteStopId = null;
            if (parcel.Status is ParcelStatus.Staged or ParcelStatus.Loaded)
            {
                parcel.Status = ParcelStatus.Sorted;

                var assigned = await binAssignmentService.AssignToBinAsync(parcel, cancellationToken);
                if (!assigned)
                {
                    parcel.Status = ParcelStatus.Exception;
                    var userName = currentUserService.UserName ?? currentUserService.UserId
                        ?? throw new InvalidOperationException("User not authenticated");

                    context.TrackingEvents.Add(new TrackingEvent
                    {
                        ParcelId = parcel.Id,
                        Timestamp = DateTimeOffset.UtcNow,
                        EventType = EventType.Exception,
                        Description = "No available bin in zone",
                        Operator = userName
                    });
                }
            }
        }

        // Remove stops that now have zero remaining parcels
        var parcelIdSet = request.ParcelIds.ToHashSet();
        var emptyStops = route.RouteStops
            .Where(s => !s.Parcels.Any(p => !parcelIdSet.Contains(p.Id)))
            .ToList();
        foreach (var stop in emptyStops)
        {
            route.RouteStops.Remove(stop);
            context.RouteStops.Remove(stop);
        }

        // Re-sequence remaining stops
        var remainingStops = route.RouteStops
            .OrderBy(s => s.SequenceNumber)
            .ToList();

        for (var i = 0; i < remainingStops.Count; i++)
        {
            remainingStops[i].SequenceNumber = i + 1;
        }

        route.RecalculateTotals();

        var depotGeo = await context.Zones
            .Where(z => z.Id == route.ZoneId)
            .Select(z => z.Depot.Address.GeoLocation)
            .FirstOrDefaultAsync(cancellationToken);
        route.RecalculateDistance(depotGeo);

        await context.SaveChangesAsync(cancellationToken);

        return route.ToDto();
    }
}
