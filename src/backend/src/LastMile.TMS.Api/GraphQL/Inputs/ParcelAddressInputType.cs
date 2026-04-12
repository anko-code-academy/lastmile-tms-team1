using HotChocolate.Types;
using LastMile.TMS.Application.Common.DTOs;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class ParcelAddressInputType : InputObjectType<AddressInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<AddressInput> descriptor)
    {
        descriptor.Name("ParcelAddressInput");

        descriptor.Field(d => d.Street1).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.Street2);
        descriptor.Field(d => d.City).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.State).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.PostalCode).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.CountryCode).DefaultValue("US");
        descriptor.Field(d => d.IsResidential).DefaultValue(false);
        descriptor.Field(d => d.ContactName);
        descriptor.Field(d => d.CompanyName);
        descriptor.Field(d => d.Phone);
        descriptor.Field(d => d.Email);
    }
}
