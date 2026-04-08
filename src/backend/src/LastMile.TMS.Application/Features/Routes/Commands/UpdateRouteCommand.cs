using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record UpdateRouteCommand(
    Guid Id,
    string Name,
    DateTime PlannedStartTime,
    Guid? ZoneId,
    Guid? VehicleId,
    Guid? DriverId
) : IRequest<RouteDto>;
