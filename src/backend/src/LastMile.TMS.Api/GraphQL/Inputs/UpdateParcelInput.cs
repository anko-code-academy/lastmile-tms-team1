using HotChocolate.Types;
using LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateParcelInput : InputObjectType<UpdateParcelCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateParcelCommand> descriptor)
    {
        descriptor.Name("UpdateParcelCommandInput");

        descriptor.Field(p => p.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(p => p.Description);
        descriptor.Field(p => p.Weight);
        descriptor.Field(p => p.Length);
        descriptor.Field(p => p.Width);
        descriptor.Field(p => p.Height);
        descriptor.Field(p => p.ServiceType);
        descriptor.Field(p => p.ParcelType);
        descriptor.Field(p => p.ShipperAddress).Type<AddressInputType>();
        descriptor.Field(p => p.RecipientAddress).Type<AddressInputType>();
    }
}
