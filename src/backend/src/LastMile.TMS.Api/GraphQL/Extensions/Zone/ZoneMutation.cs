using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;
using LastMile.TMS.Application.Features.Zones.Commands.DeleteZone;
using LastMile.TMS.Application.Features.Zones.Commands.UpdateZone;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Zone;

[ExtendObjectType(typeof(Mutation))]
public class ZoneMutation
{
    [Authorize(Roles = new string[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<ZoneResult> CreateZone(CreateZoneCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new string[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<ZoneResult> UpdateZone(UpdateZoneCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new string[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<bool> DeleteZone(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new DeleteZoneCommand(id));
}