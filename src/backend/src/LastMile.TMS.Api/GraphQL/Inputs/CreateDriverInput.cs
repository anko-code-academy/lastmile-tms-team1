using HotChocolate.Types;
using LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CreateDriverInput : InputObjectType<CreateDriverCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateDriverCommand> descriptor)
    {
        descriptor.Name("CreateDriverCommandInput");

        descriptor.Field(d => d.Email).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.LicenseNumber).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.LicenseExpiryDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(d => d.Photo).Type<StringType>();
        descriptor.Field(d => d.ShiftSchedules).Type<ListType<ShiftScheduleInputType>>();
        descriptor.Field(d => d.DaysOff).Type<ListType<DayOffInputType>>();
    }
}
