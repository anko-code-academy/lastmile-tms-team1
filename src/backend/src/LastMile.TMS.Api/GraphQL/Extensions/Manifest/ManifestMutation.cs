using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Manifests.Commands;
using LastMile.TMS.Application.Features.Manifests.Commands.CompleteManifestReceiving;
using LastMile.TMS.Application.Features.Manifests.Commands.CreateManifest;
using LastMile.TMS.Application.Features.Manifests.Commands.ReceiveParcel;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Manifest;

[ExtendObjectType(typeof(Mutation))]
public class ManifestMutation
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager,
        Role.RoleNames.WarehouseOperator })]
    public async Task<ManifestResult> CreateManifest(
        CreateManifestCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager,
        Role.RoleNames.WarehouseOperator })]
    public async Task<ReceiveParcelResult> ReceiveParcel(
        ReceiveParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager,
        Role.RoleNames.WarehouseOperator })]
    public async Task<ManifestResult> CompleteManifestReceiving(
        CompleteManifestReceivingCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);
}
