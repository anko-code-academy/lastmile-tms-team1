using HotChocolate.Types;
using LastMile.TMS.Application.Features.Zones.Commands.UpdateZone;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateZoneInput : InputObjectType<UpdateZoneCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateZoneCommand> descriptor)
    {
        descriptor.Name("UpdateZoneCommandInput");

        descriptor.Field(z => z.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(z => z.Name).Type<NonNullType<StringType>>();
        descriptor.Field(z => z.GeoJson).Type<StringType>();
        descriptor.Field(z => z.DepotId).Type<NonNullType<UuidType>>();
        descriptor.Field(z => z.IsActive).Type<NonNullType<BooleanType>>();
    }
}
