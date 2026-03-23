using HotChocolate.Types;
using LastMile.TMS.Application.Features.Zones.Queries.GetZone;

namespace LastMile.TMS.Api.GraphQL.Types;

public class ZoneType : ObjectType<ZoneDto>
{
    protected override void Configure(IObjectTypeDescriptor<ZoneDto> descriptor)
    {
        descriptor.Name("Zone");

        descriptor.Field(z => z.Id);
        descriptor.Field(z => z.Name);
        descriptor.Field(z => z.GeoJson);
        descriptor.Field(z => z.DepotId);
        descriptor.Field(z => z.DepotName);
        descriptor.Field(z => z.IsActive);
        descriptor.Field(z => z.CreatedAt);
        descriptor.Field(z => z.LastModifiedAt);
    }
}
