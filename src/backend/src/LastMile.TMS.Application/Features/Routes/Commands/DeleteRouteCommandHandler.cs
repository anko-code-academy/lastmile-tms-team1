using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class DeleteRouteCommandHandler(IAppDbContext context) : IRequestHandler<DeleteRouteCommand, bool>
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

        // Clear parcel RouteStopIds before cascade delete removes the stops
        foreach (var stop in route.RouteStops)
        {
            foreach (var parcel in stop.Parcels)
            {
                parcel.RouteStopId = null;
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
