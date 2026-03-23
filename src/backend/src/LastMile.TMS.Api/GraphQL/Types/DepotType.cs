using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Queries.GetDepot;

namespace LastMile.TMS.Api.GraphQL.Types;

public class DepotType : ObjectType<DepotDto>
{
    protected override void Configure(IObjectTypeDescriptor<DepotDto> descriptor)
    {
        descriptor.Name("Depot");

        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.Name);
        descriptor.Field(d => d.Address).Type<AddressType>();
        descriptor.Field(d => d.OperatingHours).Type<ListType<DailyOperatingHoursType>>();
        descriptor.Field(d => d.IsActive);
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt);
        descriptor.Field(d => d.ZoneIds).Type<ListType<UuidType>>();
    }
}

public class AddressType : ObjectType<AddressDto>
{
    protected override void Configure(IObjectTypeDescriptor<AddressDto> descriptor)
    {
        descriptor.Name("Address");

        descriptor.Field(a => a.Street1);
        descriptor.Field(a => a.Street2);
        descriptor.Field(a => a.City);
        descriptor.Field(a => a.State);
        descriptor.Field(a => a.PostalCode);
        descriptor.Field(a => a.CountryCode);
        descriptor.Field(a => a.IsResidential);
        descriptor.Field(a => a.ContactName);
        descriptor.Field(a => a.CompanyName);
        descriptor.Field(a => a.Phone);
        descriptor.Field(a => a.Email);
    }
}

public class DailyOperatingHoursType : ObjectType<DailyOperatingHoursDto>
{
    protected override void Configure(IObjectTypeDescriptor<DailyOperatingHoursDto> descriptor)
    {
        descriptor.Name("DailyOperatingHours");

        descriptor.Field(h => h.DayOfWeek);
        descriptor.Field(h => h.OpenTime);
        descriptor.Field(h => h.CloseTime);
    }
}
