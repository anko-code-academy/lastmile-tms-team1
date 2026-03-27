using MediatR;

namespace LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

public record ZoneResult(
    Guid Id,
    string Name,
    Guid DepotId,
    bool IsActive,
    DateTimeOffset CreatedAt);

public record CreateZoneCommand(
    string Name,
    string GeoJson,
    Guid DepotId,
    bool IsActive = true) : IRequest<ZoneResult>;
