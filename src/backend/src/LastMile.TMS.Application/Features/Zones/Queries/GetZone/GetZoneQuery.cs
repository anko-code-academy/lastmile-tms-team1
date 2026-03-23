using MediatR;

namespace LastMile.TMS.Application.Features.Zones.Queries.GetZone;

public record GetZoneQuery(Guid Id) : IRequest<ZoneDto?>;
