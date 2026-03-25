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
        var route = new Route
        {
            Name = request.Name,
            Status = RouteStatus.Planned,
            PlannedStartTime = request.PlannedStartTime,
            TotalDistanceKm = request.TotalDistanceKm,
            TotalParcelCount = request.TotalParcelCount,
            VehicleId = request.VehicleId
        };

        context.Routes.Add(route);

        if (request.VehicleId.HasValue)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
            if (vehicle != null)
            {
                if (vehicle.Status == Domain.Enums.VehicleStatus.Retired)
                {
                    throw new InvalidOperationException("Cannot assign a retired vehicle to a route");
                }
                vehicle.Status = Domain.Enums.VehicleStatus.InUse;
            }
        }

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
            CreatedAt = route.CreatedAt
        };
    }
}
