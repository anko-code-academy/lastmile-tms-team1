using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class CreateRouteCommandHandler(IAppDbContext context) : IRequestHandler<CreateRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        if (request.VehicleId.HasValue)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
            if (vehicle == null)
            {
                throw new InvalidOperationException($"Vehicle with ID {request.VehicleId.Value} not found.");
            }
            if (vehicle.Status == VehicleStatus.Retired)
            {
                throw new InvalidOperationException("Cannot assign a retired vehicle to a route.");
            }
        }

        string? driverName = null;
        if (request.DriverId.HasValue)
        {
            var driver = await context.Drivers
                .Include(d => d.User)
                .Include(d => d.DaysOff)
                .FirstOrDefaultAsync(d => d.Id == request.DriverId.Value, cancellationToken);

            if (driver == null)
            {
                throw new InvalidOperationException($"Driver with ID {request.DriverId.Value} not found.");
            }

            DriverDayOffValidator.EnsureAvailableForDate(driver, request.PlannedStartTime);

            driverName = $"{driver.User.FirstName} {driver.User.LastName}";
        }

        string? zoneName = null;
        if (request.ZoneId.HasValue)
        {
            var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == request.ZoneId.Value, cancellationToken);
            if (zone == null)
            {
                throw new InvalidOperationException($"Zone with ID {request.ZoneId.Value} not found.");
            }
            zoneName = zone.Name;
        }

        var route = new Route
        {
            Name = request.Name,
            Status = RouteStatus.Draft,
            PlannedStartTime = request.PlannedStartTime,
            TotalDistanceKm = 0,
            TotalParcelCount = 0,
            ZoneId = request.ZoneId,
            VehicleId = request.VehicleId,
            DriverId = request.DriverId
        };

        context.Routes.Add(route);
        await context.SaveChangesAsync(cancellationToken);

        var vehiclePlate = request.VehicleId.HasValue
            ? await context.Vehicles.Where(v => v.Id == request.VehicleId.Value).Select(v => v.RegistrationPlate).FirstOrDefaultAsync(cancellationToken)
            : null;

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
            VehiclePlate = vehiclePlate,
            DriverId = route.DriverId,
            DriverName = driverName,
            ZoneId = route.ZoneId,
            ZoneName = zoneName,
            EstimatedStopCount = 0,
            CreatedAt = route.CreatedAt
        };
    }
}
