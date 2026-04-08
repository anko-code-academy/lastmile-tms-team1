using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record AddParcelsToRouteCommand(
    Guid RouteId,
    List<Guid> ParcelIds
) : IRequest<RouteDto>;
