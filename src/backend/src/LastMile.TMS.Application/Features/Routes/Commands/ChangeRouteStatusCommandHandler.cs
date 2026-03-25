using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class ChangeRouteStatusCommandHandler(IAppDbContext context) : IRequestHandler<ChangeRouteStatusCommand, RouteDto>
{
    public async Task<RouteDto> Handle(ChangeRouteStatusCommand request, CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.Id} not found");
        }

        var oldVehicleId = route.VehicleId;

        // Handle status transitions
        if (request.NewStatus == RouteStatus.Completed)
        {
            route.ActualEndTime = DateTime.UtcNow;

            // Create/update VehicleJourney for history tracking
            if (oldVehicleId.HasValue)
            {
                var journey = await context.VehicleJourneys
                    .FirstOrDefaultAsync(j => j.RouteId == route.Id && j.VehicleId == oldVehicleId.Value, cancellationToken);

                if (journey != null)
                {
                    journey.EndTime = DateTime.UtcNow;
                    // Use route distance as placeholder until telematics provides actual mileage
                    journey.EndMileageKm = journey.StartMileageKm + route.TotalDistanceKm;
                }

                // Release vehicle when route is completed
                await ReleaseVehicleIfNotUsedAsync(oldVehicleId.Value, request.Id, cancellationToken);
            }
        }
        else if (request.NewStatus == RouteStatus.Cancelled)
        {
            // Release vehicle when route is cancelled
            if (oldVehicleId.HasValue)
            {
                await ReleaseVehicleIfNotUsedAsync(oldVehicleId.Value, request.Id, cancellationToken);
            }
        }
        else if (request.NewStatus == RouteStatus.InProgress)
        {
            route.ActualStartTime = DateTime.UtcNow;

            // Create VehicleJourney for history tracking
            if (oldVehicleId.HasValue)
            {
                var existingJourney = await context.VehicleJourneys
                    .FirstOrDefaultAsync(j => j.RouteId == route.Id && j.VehicleId == oldVehicleId.Value, cancellationToken);

                if (existingJourney == null)
                {
                    var journey = new VehicleJourney
                    {
                        RouteId = route.Id,
                        VehicleId = oldVehicleId.Value,
                        StartTime = DateTime.UtcNow,
                        StartMileageKm = 0 // Would come from vehicle telematics or driver check-in
                    };
                    context.VehicleJourneys.Add(journey);
                }

                // Assign vehicle if not already assigned
                var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == oldVehicleId.Value, cancellationToken);
                if (vehicle != null && vehicle.Status == Domain.Enums.VehicleStatus.Available)
                {
                    vehicle.Status = Domain.Enums.VehicleStatus.InUse;
                }
            }
        }

        route.Status = request.NewStatus;

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

    private async Task ReleaseVehicleIfNotUsedAsync(Guid vehicleId, Guid excludeRouteId, CancellationToken cancellationToken)
    {
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken);
        if (vehicle != null && vehicle.Status == Domain.Enums.VehicleStatus.InUse)
        {
            var stillInUse = await context.Routes.AnyAsync(r => r.VehicleId == vehicleId && r.Id != excludeRouteId, cancellationToken);
            if (!stillInUse)
            {
                vehicle.Status = Domain.Enums.VehicleStatus.Available;
            }
        }
    }
}
