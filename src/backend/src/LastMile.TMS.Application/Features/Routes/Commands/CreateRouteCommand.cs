using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record CreateRouteCommand(
    string Name,
    DateTime PlannedStartTime,
    Guid? ZoneId,
    Guid? VehicleId,
    Guid? DriverId
) : IRequest<RouteDto>;
