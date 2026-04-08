using HotChocolate.Types;
using LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CreateParcelInput : InputObjectType<CreateParcelCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateParcelCommand> descriptor)
    {
        descriptor.Name("CreateParcelCommandInput");

        descriptor.Field(d => d.Description);
        descriptor.Field(d => d.ServiceType).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.ShipperAddress).Type<NonNullType<ParcelAddressInputType>>();
        descriptor.Field(d => d.RecipientAddress).Type<NonNullType<ParcelAddressInputType>>();
        descriptor.Field(d => d.Weight).Type<NonNullType<DecimalType>>();
        descriptor.Field(d => d.WeightUnit).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.Length).Type<NonNullType<DecimalType>>();
        descriptor.Field(d => d.Width).Type<NonNullType<DecimalType>>();
        descriptor.Field(d => d.Height).Type<NonNullType<DecimalType>>();
        descriptor.Field(d => d.DimensionUnit).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.DeclaredValue).Type<DecimalType>();
        descriptor.Field(d => d.Currency).DefaultValue("USD");
        descriptor.Field(d => d.ParcelType);
        descriptor.Field(d => d.Notes);
    }
}
