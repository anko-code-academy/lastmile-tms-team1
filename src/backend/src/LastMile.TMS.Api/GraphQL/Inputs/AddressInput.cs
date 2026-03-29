using HotChocolate.Types;
using AddressInput = LastMile.TMS.Application.Features.Depots.Commands.CreateDepot.AddressInput;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class AddressInputType : InputObjectType<AddressInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<AddressInput> descriptor)
    {
        descriptor.Name("AddressInput");

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
