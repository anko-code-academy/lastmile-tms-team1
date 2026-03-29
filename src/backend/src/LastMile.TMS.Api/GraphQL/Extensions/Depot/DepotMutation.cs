using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;
using LastMile.TMS.Application.Features.Depots.Commands.DeleteDepot;
using LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Depot;

[ExtendObjectType(typeof(Mutation))]
public class DepotMutation
{
    [Authorize(Roles = new string[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<DepotResult> CreateDepot(CreateDepotCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new string[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<DepotResult> UpdateDepot(UpdateDepotCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new string[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<bool> DeleteDepot(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new DeleteDepotCommand(id));
}