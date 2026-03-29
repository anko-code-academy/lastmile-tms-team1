using MediatR;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record CreateRouteCommand(
    string Name,
    DateTime PlannedStartTime,
    decimal TotalDistanceKm,
    int TotalParcelCount,
    Guid? VehicleId
) : IRequest<RouteDto>;
