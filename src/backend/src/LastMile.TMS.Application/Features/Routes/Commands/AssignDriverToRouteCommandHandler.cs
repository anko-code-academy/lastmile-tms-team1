using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class AssignDriverToRouteCommandHandler(IAppDbContext context) : IRequestHandler<AssignDriverToRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(AssignDriverToRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .Include(r => r.Vehicle)
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.RouteId} not found");
        }

        route.AssignDriver(request.DriverId);

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

            DriverDayOffValidator.EnsureAvailableForDate(driver, route.PlannedStartTime);

            route.Driver = driver;
        }
        else
        {
            route.Driver = null;
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
            DriverId = route.DriverId,
            DriverName = route.Driver != null
                ? $"{route.Driver.User.FirstName} {route.Driver.User.LastName}"
                : null,
            CreatedAt = route.CreatedAt
        };
    }
}
