using MediatR;

namespace LastMile.TMS.Application.Features.Zones.Queries.GetAllZones;

public record GetAllZonesQuery : IRequest<List<ZoneSummaryDto>>;
