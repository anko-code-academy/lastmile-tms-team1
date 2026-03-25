using HotChocolate.Types;
using DailyOperatingHoursInput = LastMile.TMS.Application.Features.Depots.Commands.CreateDepot.DailyOperatingHoursInput;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class DailyOperatingHoursInputType : InputObjectType<DailyOperatingHoursInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<DailyOperatingHoursInput> descriptor)
    {
        descriptor.Name("DailyOperatingHoursInput");

        descriptor.Field(d => d.DayOfWeek);
        descriptor.Field(d => d.OpenTime).Type<LocalTimeType>();
        descriptor.Field(d => d.CloseTime).Type<LocalTimeType>();
    }
}
