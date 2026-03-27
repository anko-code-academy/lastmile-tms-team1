using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class UpdateRouteCommandHandler(IAppDbContext context) : IRequestHandler<UpdateRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.Id} not found");
        }

        var oldParcelCount = route.TotalParcelCount;
        var oldVehicleId = route.VehicleId;

        route.Name = request.Name;
        route.PlannedStartTime = request.PlannedStartTime;
        route.TotalDistanceKm = request.TotalDistanceKm;
        route.TotalParcelCount = request.TotalParcelCount;
        route.VehicleId = request.VehicleId;

        // Update vehicle statuses
        if (oldVehicleId != request.VehicleId)
        {
            if (oldVehicleId.HasValue)
            {
                var oldVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == oldVehicleId.Value, cancellationToken);
                if (oldVehicle != null && oldVehicle.Status == Domain.Enums.VehicleStatus.InUse)
                {
                    // Check if vehicle is still assigned to other routes
                    var stillInUse = await context.Routes.AnyAsync(r => r.VehicleId == oldVehicleId.Value && r.Id != request.Id, cancellationToken);
                    if (!stillInUse)
                    {
                        oldVehicle.ReleaseFromRoute();
                    }
                }
            }

            if (request.VehicleId.HasValue)
            {
                var newVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
                if (newVehicle == null)
                {
                    throw new InvalidOperationException($"Vehicle with ID {request.VehicleId.Value} not found.");
                }
                newVehicle.AssignToRoute(request.TotalParcelCount);
            }
        }
        else if (request.VehicleId.HasValue && oldParcelCount != request.TotalParcelCount)
        {
            // If vehicle is the same but parcel count changed, re-validate capacity
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
            if (vehicle == null)
            {
                throw new InvalidOperationException($"Vehicle with ID {request.VehicleId.Value} not found.");
            }
            vehicle.AssignToRoute(request.TotalParcelCount);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new RouteDto
        {
            Id = route.Id,
            Name = route.Name,
            Status = route.Status,
            PlannedStartTime = route.PlannedStartTime,
            ActualStartTime = route.ActualStartTime,
            ActualEndTime = route.ActualEndTime,
            TotalDistanceKm = route.TotalDistanceKm,
            TotalParcelCount = route.TotalParcelCount,
            VehicleId = route.VehicleId,
            VehiclePlate = route.Vehicle?.RegistrationPlate,
            CreatedAt = route.CreatedAt
        };
    }
}
