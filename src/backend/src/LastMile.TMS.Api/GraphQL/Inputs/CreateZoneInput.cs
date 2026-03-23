using HotChocolate.Types;
using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CreateZoneInput : InputObjectType<CreateZoneCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateZoneCommand> descriptor)
    {
        descriptor.Name("CreateZoneInput");

        descriptor.Field(z => z.Name).Type<NonNullType<StringType>>();
        descriptor.Field(z => z.GeoJson).Type<NonNullType<StringType>>();
        descriptor.Field(z => z.DepotId).Type<NonNullType<UuidType>>();
        descriptor.Field(z => z.IsActive).DefaultValue(true);
    }
}
