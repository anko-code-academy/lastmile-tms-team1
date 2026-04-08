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
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.Id} not found");
        }

        var oldVehicleId = route.VehicleId;

        // Handle vehicle release when route completes
        if (request.NewStatus == RouteStatus.Completed && oldVehicleId.HasValue)
        {
            // Create/update VehicleJourney for history tracking
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
        else if (request.NewStatus == RouteStatus.InProgress && oldVehicleId.HasValue)
        {
            // Create VehicleJourney for history tracking
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
            if (vehicle == null)
            {
                throw new InvalidOperationException($"Vehicle with ID {oldVehicleId.Value} not found.");
            }
            vehicle.AssignToRoute(route.TotalParcelCount);
        }

        // Use domain logic for status transition (validates and sets timestamps)
        route.TransitionTo(request.NewStatus);

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
            DriverId = route.DriverId,
            DriverName = route.Driver != null
                ? $"{route.Driver.User.FirstName} {route.Driver.User.LastName}"
                : null,
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
                vehicle.ReleaseFromRoute();
            }
        }
    }
}
