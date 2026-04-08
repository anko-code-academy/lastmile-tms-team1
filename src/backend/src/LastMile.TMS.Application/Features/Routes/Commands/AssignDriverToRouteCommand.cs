using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record AssignDriverToRouteCommand(
    Guid RouteId,
    Guid? DriverId
) : IRequest<RouteDto>;
