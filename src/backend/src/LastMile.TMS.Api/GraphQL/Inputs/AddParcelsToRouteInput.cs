using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class AddParcelsToRouteInput : InputObjectType<AddParcelsToRouteCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<AddParcelsToRouteCommand> descriptor)
    {
        descriptor.Name("AddParcelsToRouteCommandInput");

        descriptor.Field(x => x.RouteId).Type<NonNullType<UuidType>>();
        descriptor.Field(x => x.ParcelIds).Type<NonNullType<ListType<NonNullType<UuidType>>>>();
    }
}
