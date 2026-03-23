using HotChocolate.Types;
using LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

namespace LastMile.TMS.Api.GraphQL.Types;

public class ZoneResultType : ObjectType<ZoneResult>
{
    protected override void Configure(IObjectTypeDescriptor<ZoneResult> descriptor)
    {
        descriptor.Name("ZoneResult");

        descriptor.Field(z => z.Id);
        descriptor.Field(z => z.Name);
        descriptor.Field(z => z.DepotId);
        descriptor.Field(z => z.IsActive);
        descriptor.Field(z => z.CreatedAt);
    }
}
