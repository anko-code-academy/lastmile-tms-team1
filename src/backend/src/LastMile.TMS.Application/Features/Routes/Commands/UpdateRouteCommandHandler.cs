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

        var oldVehicleId = route.VehicleId;

        route.Name = request.Name;
        route.PlannedStartTime = request.PlannedStartTime;
        route.TotalDistanceKm = request.TotalDistanceKm;
        route.TotalParcelCount = request.TotalParcelCount;
        route.VehicleId = request.VehicleId;

        // Update vehicle statuses
        if (oldVehicleId.HasValue && oldVehicleId != request.VehicleId)
        {
            var oldVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == oldVehicleId.Value, cancellationToken);
            if (oldVehicle != null && oldVehicle.Status == Domain.Enums.VehicleStatus.InUse)
            {
                // Check if vehicle is still assigned to other routes
                var stillInUse = await context.Routes.AnyAsync(r => r.VehicleId == oldVehicleId.Value && r.Id != request.Id, cancellationToken);
                if (!stillInUse)
                {
                    oldVehicle.Status = Domain.Enums.VehicleStatus.Available;
                }
            }
        }

        if (request.VehicleId.HasValue && oldVehicleId != request.VehicleId)
        {
            var newVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
            if (newVehicle != null)
            {
                if (newVehicle.Status == Domain.Enums.VehicleStatus.Retired)
                {
                    throw new InvalidOperationException("Cannot assign a retired vehicle to a route");
                }
                newVehicle.Status = Domain.Enums.VehicleStatus.InUse;
            }
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
