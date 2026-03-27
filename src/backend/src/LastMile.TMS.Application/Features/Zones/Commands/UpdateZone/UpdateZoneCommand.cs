using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;
using MediatR;

namespace LastMile.TMS.Application.Features.Zones.Commands.UpdateZone;

public record UpdateZoneCommand(
    Guid Id,
    string Name,
    string? GeoJson,
    Guid DepotId,
    bool IsActive) : IRequest<ZoneResult>;
