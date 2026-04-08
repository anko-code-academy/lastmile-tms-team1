using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Application.Features.Routes.Commands;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Route;

[ExtendObjectType(typeof(Mutation))]
public class RouteMutation
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> CreateRoute(CreateRouteCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> UpdateRoute(UpdateRouteCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<bool> DeleteRoute(
        [Service] IMediator mediator,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new DeleteRouteCommand(id), cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> ChangeRouteStatus(
        [Service] IMediator mediator,
        Guid id,
        RouteStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new ChangeRouteStatusCommand(id, newStatus), cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> AssignDriverToRoute(
        [Service] IMediator mediator,
        Guid routeId,
        Guid? driverId = null,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(
            new AssignDriverToRouteCommand(routeId, driverId),
            cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> AddParcelsToRoute(AddParcelsToRouteCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> AutoAssignParcelsByZone(AutoAssignParcelsByZoneCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> RemoveParcelsFromRoute(RemoveParcelsFromRouteCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public async Task<RouteDto> ReorderRouteStops(ReorderRouteStopsCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);
}
