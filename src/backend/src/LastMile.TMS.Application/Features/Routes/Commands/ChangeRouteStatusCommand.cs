using MediatR;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record ChangeRouteStatusCommand(
    Guid Id,
    RouteStatus NewStatus
) : IRequest<RouteDto>;
