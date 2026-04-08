using MediatR;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record UpdateRouteCommand(
    Guid Id,
    string Name,
    DateTime PlannedStartTime,
    decimal TotalDistanceKm,
    int TotalParcelCount,
    Guid? VehicleId,
    Guid? DriverId
) : IRequest<RouteDto>;
