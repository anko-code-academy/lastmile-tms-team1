using HotChocolate.Types;
using UpdateAddressInput = LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot.UpdateAddressInput;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateAddressInputType : InputObjectType<UpdateAddressInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateAddressInput> descriptor)
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
