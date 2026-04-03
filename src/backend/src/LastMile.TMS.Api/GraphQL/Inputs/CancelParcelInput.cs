using HotChocolate.Types;
using LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CancelParcelInput : InputObjectType<CancelParcelCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CancelParcelCommand> descriptor)
    {
        descriptor.Name("CancelParcelCommandInput");
    }
}
