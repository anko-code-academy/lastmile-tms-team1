using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Queries.GetAllDepots;
using LastMile.TMS.Application.Features.Depots.Queries.GetDepot;
using LastMile.TMS.Application.Features.Zones.Queries.GetAllZones;
using LastMile.TMS.Application.Features.Zones.Queries.GetZone;
using MediatR;

namespace LastMile.TMS.Api.GraphQL;

public class Query
{
    public async Task<DepotDto?> GetDepot(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new GetDepotQuery(id));

    public async Task<List<DepotSummaryDto>> GetDepots([Service] IMediator mediator)
        => await mediator.Send(new GetAllDepotsQuery());

    public async Task<ZoneDto?> GetZone(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new GetZoneQuery(id));

    public async Task<List<ZoneSummaryDto>> GetZones([Service] IMediator mediator)
        => await mediator.Send(new GetAllZonesQuery());
}
