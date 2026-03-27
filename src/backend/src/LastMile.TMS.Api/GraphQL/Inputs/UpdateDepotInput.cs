using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;
using DailyOperatingHoursInput = LastMile.TMS.Application.Features.Depots.Commands.CreateDepot.DailyOperatingHoursInput;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateDepotInput : InputObjectType<UpdateDepotCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateDepotCommand> descriptor)
    {
        descriptor.Name("UpdateDepotCommandInput");

        descriptor.Field(d => d.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(d => d.Name).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.IsActive).Type<NonNullType<BooleanType>>();
        descriptor.Field(d => d.Address).Type<NonNullType<UpdateAddressInputType>>();
        descriptor.Field(d => d.OperatingHours).Type<ListType<DailyOperatingHoursInputType>>();
    }
}
