using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateRouteInput : InputObjectType<UpdateRouteCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateRouteCommand> descriptor)
    {
        descriptor.Name("UpdateRouteCommandInput");

        descriptor.Field(r => r.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(r => r.Name).Type<NonNullType<StringType>>();
        descriptor.Field(r => r.PlannedStartTime).Type<NonNullType<DateTimeType>>();
    }
}
