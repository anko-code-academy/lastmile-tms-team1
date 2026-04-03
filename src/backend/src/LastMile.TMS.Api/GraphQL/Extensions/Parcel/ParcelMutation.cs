using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;
using LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Parcel;

[ExtendObjectType(typeof(Mutation))]
public class ParcelMutation
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.WarehouseOperator })]
    public async Task<ParcelResult> UpdateParcel(UpdateParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.WarehouseOperator })]
    public async Task<CancelParcelResult> CancelParcel(CancelParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);
}
