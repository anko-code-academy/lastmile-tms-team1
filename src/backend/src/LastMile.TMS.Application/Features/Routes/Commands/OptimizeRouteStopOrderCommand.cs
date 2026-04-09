using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record OptimizeRouteStopOrderCommand(
    Guid RouteId
) : IRequest<RouteDto>;
