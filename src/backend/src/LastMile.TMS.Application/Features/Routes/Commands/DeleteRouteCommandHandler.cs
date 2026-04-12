using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

using ParcelStatus = LastMile.TMS.Domain.Enums.ParcelStatus;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class DeleteRouteCommandHandler(
    IAppDbContext context,
    IBinAssignmentService binAssignmentService,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteRouteCommand, bool>
{
    public async Task<bool> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .Include(r => r.Vehicle)
            .Include(r => r.RouteStops).ThenInclude(s => s.Parcels)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
        {
            return false;
        }

        var oldVehicleId = route.VehicleId;

        // Clear parcel RouteStopIds and transition back to Sorted before cascade delete removes the stops
        foreach (var stop in route.RouteStops)
        {
            foreach (var parcel in stop.Parcels)
            {
                parcel.RouteStopId = null;
                if (parcel.Status == ParcelStatus.Staged)
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
        }

        context.Routes.Remove(route);

        // Update vehicle status if needed
        if (oldVehicleId.HasValue)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == oldVehicleId.Value, cancellationToken);
            if (vehicle != null && vehicle.Status == Domain.Enums.VehicleStatus.InUse)
            {
                var stillInUse = await context.Routes.AnyAsync(r => r.VehicleId == oldVehicleId.Value, cancellationToken);
                if (!stillInUse)
                {
                    vehicle.ReleaseFromRoute();
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
