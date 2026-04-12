using HotChocolate.Types;
using LastMile.TMS.Application.Common.DTOs;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateAddressInputType : InputObjectType<AddressInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<AddressInput> descriptor)
    {
        descriptor.Name("UpdateAddressInput");

        descriptor.Field(d => d.Street1);
        descriptor.Field(d => d.Street2);
        descriptor.Field(d => d.City);
        descriptor.Field(d => d.State);
        descriptor.Field(d => d.PostalCode);
        descriptor.Field(d => d.CountryCode);
        descriptor.Field(d => d.IsResidential);
        descriptor.Field(d => d.ContactName);
        descriptor.Field(d => d.CompanyName);
        descriptor.Field(d => d.Phone);
        descriptor.Field(d => d.Email);
    }
}
