using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Parcel;

[ExtendObjectType(typeof(Mutation))]
public class ParcelMutation
{
    [Authorize(Roles = new[] { Role.RoleNames.WarehouseOperator, Role.RoleNames.Admin })]
    public async Task<ParcelResult> CreateParcel(CreateParcelCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);
}
