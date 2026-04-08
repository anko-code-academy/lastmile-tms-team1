using HotChocolate.Types;
using ShiftScheduleInput = LastMile.TMS.Application.Features.Drivers.Common.ShiftScheduleInput;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class ShiftScheduleInputType : InputObjectType<ShiftScheduleInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<ShiftScheduleInput> descriptor)
    {
        descriptor.Name("ShiftScheduleInput");
        descriptor.Field(s => s.DayOfWeek).Type<EnumType<DayOfWeek>>();
        descriptor.Field(s => s.OpenTime).Type<LocalTimeType>();
        descriptor.Field(s => s.CloseTime).Type<LocalTimeType>();
    }
}
