using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

namespace LastMile.TMS.Api.GraphQL.Types;

public class DepotResultType : ObjectType<DepotResult>
{
    protected override void Configure(IObjectTypeDescriptor<DepotResult> descriptor)
    {
        descriptor.Name("DepotResult");

        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.Name);
        descriptor.Field(d => d.IsActive);
        descriptor.Field(d => d.CreatedAt);
    }
}
