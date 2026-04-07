using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CreateRouteInput : InputObjectType<CreateRouteCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateRouteCommand> descriptor)
    {
        descriptor.Name("CreateRouteCommandInput");

        descriptor.Field(r => r.Name).Type<NonNullType<StringType>>();
        descriptor.Field(r => r.PlannedStartTime).Type<NonNullType<DateTimeType>>();
        descriptor.Field(r => r.TotalDistanceKm).Type<NonNullType<DecimalType>>();
        descriptor.Field(r => r.TotalParcelCount).Type<NonNullType<IntType>>();
    }
}
