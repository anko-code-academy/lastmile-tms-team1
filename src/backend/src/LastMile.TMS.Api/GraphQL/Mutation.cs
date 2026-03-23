using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;
using LastMile.TMS.Application.Features.Depots.Commands.DeleteDepot;
using LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;
using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;
using LastMile.TMS.Application.Features.Zones.Commands.DeleteZone;
using LastMile.TMS.Application.Features.Zones.Commands.UpdateZone;
using MediatR;

namespace LastMile.TMS.Api.GraphQL;

public class Mutation
{
    public async Task<DepotResult> CreateDepot(CreateDepotCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    public async Task<DepotResult> UpdateDepot(UpdateDepotCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    public async Task<bool> DeleteDepot(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new DeleteDepotCommand(id));

    public async Task<ZoneResult> CreateZone(CreateZoneCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    public async Task<ZoneResult> UpdateZone(UpdateZoneCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    public async Task<bool> DeleteZone(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new DeleteZoneCommand(id));
}
