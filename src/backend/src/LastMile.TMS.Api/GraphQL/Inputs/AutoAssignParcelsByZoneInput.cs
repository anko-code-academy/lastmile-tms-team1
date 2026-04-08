using HotChocolate.Types;
using LastMile.TMS.Application.Features.Routes.Commands;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class AutoAssignParcelsByZoneInput : InputObjectType<AutoAssignParcelsByZoneCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<AutoAssignParcelsByZoneCommand> descriptor)
    {
        descriptor.Name("AutoAssignParcelsByZoneCommandInput");

        descriptor.Field(x => x.RouteId).Type<NonNullType<UuidType>>();
    }
}
