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
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
        {
            return false;
        }

        var oldVehicleId = route.VehicleId;

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
                    vehicle.Status = Domain.Enums.VehicleStatus.Available;
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
