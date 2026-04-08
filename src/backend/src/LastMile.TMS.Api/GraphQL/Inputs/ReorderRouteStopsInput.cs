using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class ReorderRouteStopsInput : InputObjectType<ReorderRouteStopsCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<ReorderRouteStopsCommand> descriptor)
    {
        descriptor.Name("ReorderRouteStopsCommandInput");

        descriptor.Field(x => x.RouteId).Type<NonNullType<UuidType>>();
        descriptor.Field(x => x.StopIdsInOrder).Type<NonNullType<ListType<NonNullType<UuidType>>>>();
    }
}
