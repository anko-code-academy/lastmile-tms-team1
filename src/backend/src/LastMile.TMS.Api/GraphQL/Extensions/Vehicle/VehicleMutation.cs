using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Vehicles;
using LastMile.TMS.Application.Features.Vehicles.Commands;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Vehicle;

[ExtendObjectType(typeof(Mutation))]
public class VehicleMutation
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<VehicleDto> CreateVehicle(
        [Service] IMediator mediator,
        string registrationPlate,
        VehicleType type,
        int parcelCapacity,
        decimal weightCapacityKg,
        Guid? depotId = null,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(
            new CreateVehicleCommand(registrationPlate, type, parcelCapacity, weightCapacityKg, depotId),
            cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<VehicleDto> UpdateVehicle(
        [Service] IMediator mediator,
        Guid id,
        string registrationPlate,
        VehicleType type,
        int parcelCapacity,
        decimal weightCapacityKg,
        Guid? depotId = null,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(
            new UpdateVehicleCommand(id, registrationPlate, type, parcelCapacity, weightCapacityKg, depotId),
            cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<bool> DeleteVehicle(
        [Service] IMediator mediator,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new DeleteVehicleCommand(id), cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<VehicleDto> ChangeVehicleStatus(
        [Service] IMediator mediator,
        Guid id,
        VehicleStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new ChangeVehicleStatusCommand(id, newStatus), cancellationToken);
    }
}
