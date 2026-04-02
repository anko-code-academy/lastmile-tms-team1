using HotChocolate.Types;
using LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateDriverInput : InputObjectType<UpdateDriverCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateDriverCommand> descriptor)
    {
        descriptor.Name("UpdateDriverCommandInput");

        descriptor.Field(d => d.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(d => d.LicenseNumber).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.LicenseExpiryDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(d => d.Photo).Type<StringType>();
        descriptor.Field(d => d.ShiftSchedules).Type<ListType<ShiftScheduleInputType>>();
        descriptor.Field(d => d.DaysOff).Type<ListType<DayOffInputType>>();
    }
}
