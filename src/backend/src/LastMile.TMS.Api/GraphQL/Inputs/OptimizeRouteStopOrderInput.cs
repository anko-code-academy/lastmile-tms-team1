using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class OptimizeRouteStopOrderInput : InputObjectType<OptimizeRouteStopOrderCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<OptimizeRouteStopOrderCommand> descriptor)
    {
        descriptor.Name("OptimizeRouteStopOrderCommandInput");

        descriptor.Field(x => x.RouteId).Type<NonNullType<UuidType>>();
    }
}
