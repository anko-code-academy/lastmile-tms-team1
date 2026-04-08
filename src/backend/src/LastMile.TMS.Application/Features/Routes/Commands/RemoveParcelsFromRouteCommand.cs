using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record RemoveParcelsFromRouteCommand(
    Guid RouteId,
    List<Guid> ParcelIds
) : IRequest<RouteDto>;
