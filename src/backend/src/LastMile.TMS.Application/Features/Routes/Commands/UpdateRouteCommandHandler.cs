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
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .Include(r => r.RouteStops).ThenInclude(s => s.Parcels)
            .Include(r => r.Zone)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.Id} not found");
        }

        var oldVehicleId = route.VehicleId;

        route.Name = request.Name;
        route.PlannedStartTime = request.PlannedStartTime;
        route.VehicleId = request.VehicleId;
        route.ZoneId = request.ZoneId;

        // Only update vehicle assignment when route is InProgress
        if (route.Status == RouteStatus.InProgress)
        {
            if (oldVehicleId != request.VehicleId)
            {
                if (oldVehicleId.HasValue)
                {
                    var oldVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == oldVehicleId.Value, cancellationToken);
                    if (oldVehicle != null && oldVehicle.Status == Domain.Enums.VehicleStatus.InUse)
                    {
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
                    newVehicle.AssignToRoute(route.TotalParcelCount);
                }
            }
        }
        else if (oldVehicleId != request.VehicleId && request.VehicleId.HasValue)
        {
            var newVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
            if (newVehicle == null)
            {
                throw new InvalidOperationException($"Vehicle with ID {request.VehicleId.Value} not found.");
            }
            if (newVehicle.Status == Domain.Enums.VehicleStatus.Retired)
            {
                throw new InvalidOperationException("Cannot assign a retired vehicle to a route.");
            }
            route.Vehicle = newVehicle;
        }

        // Driver assignment — only allowed for Draft routes
        if (route.DriverId != request.DriverId)
        {
            if (request.DriverId.HasValue)
            {
                var driver = await context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.DaysOff)
                    .FirstOrDefaultAsync(d => d.Id == request.DriverId!.Value, cancellationToken);

                if (driver == null)
                {
                    throw new InvalidOperationException($"Driver with ID {request.DriverId.Value} not found.");
                }

                DriverDayOffValidator.EnsureAvailableForDate(driver, request.PlannedStartTime);

                route.AssignDriver(request.DriverId);
                route.Driver = driver;
            }
            else
            {
                route.AssignDriver(request.DriverId);
                route.Driver = null;
            }
        }

        route.RecalculateTotals();
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
            ZoneId = route.ZoneId,
            ZoneName = route.Zone?.Name,
            EstimatedStopCount = route.RouteStops.Count,
            CreatedAt = route.CreatedAt
        };
    }
}
