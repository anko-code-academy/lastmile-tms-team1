using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class RemoveParcelsFromRouteInput : InputObjectType<RemoveParcelsFromRouteCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<RemoveParcelsFromRouteCommand> descriptor)
    {
        descriptor.Name("RemoveParcelsFromRouteCommandInput");

        descriptor.Field(x => x.RouteId).Type<NonNullType<UuidType>>();
        descriptor.Field(x => x.ParcelIds).Type<NonNullType<ListType<NonNullType<UuidType>>>>();
    }
}
