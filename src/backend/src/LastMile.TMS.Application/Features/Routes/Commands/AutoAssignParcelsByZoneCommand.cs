using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record AutoAssignParcelsByZoneCommand(
    Guid RouteId
) : IRequest<RouteDto>;
