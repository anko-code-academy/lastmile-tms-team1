using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;
using LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;
using LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Parcel;

[ExtendObjectType(typeof(Mutation))]
public class ParcelMutation
{
    [Authorize]
    public async Task<UpdateParcelResult> UpdateParcel(UpdateParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize]
    public async Task<CancelParcelResult> CancelParcel(CancelParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new[] { Role.RoleNames.WarehouseOperator, Role.RoleNames.Admin })]
    public async Task<CreateParcelResult> CreateParcel(CreateParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);
}
