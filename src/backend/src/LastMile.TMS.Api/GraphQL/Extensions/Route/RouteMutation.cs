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
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<RouteDto> CreateRoute(
        [Service] IMediator mediator,
        string name,
        DateTime plannedStartTime,
        decimal totalDistanceKm,
        int totalParcelCount,
        Guid? vehicleId = null,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(
            new CreateRouteCommand(name, plannedStartTime, totalDistanceKm, totalParcelCount, vehicleId),
            cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<RouteDto> UpdateRoute(
        [Service] IMediator mediator,
        Guid id,
        string name,
        DateTime plannedStartTime,
        decimal totalDistanceKm,
        int totalParcelCount,
        Guid? vehicleId = null,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(
            new UpdateRouteCommand(id, name, plannedStartTime, totalDistanceKm, totalParcelCount, vehicleId),
            cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<bool> DeleteRoute(
        [Service] IMediator mediator,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new DeleteRouteCommand(id), cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<RouteDto> ChangeRouteStatus(
        [Service] IMediator mediator,
        Guid id,
        RouteStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new ChangeRouteStatusCommand(id, newStatus), cancellationToken);
    }
}
