using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record ReorderRouteStopsCommand(
    Guid RouteId,
    List<Guid> StopIdsInOrder
) : IRequest<RouteDto>;
