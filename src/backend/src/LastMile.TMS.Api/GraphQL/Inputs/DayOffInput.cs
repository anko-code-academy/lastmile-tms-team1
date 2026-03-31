using HotChocolate.Types;
using DayOffInput = LastMile.TMS.Application.Features.Drivers.Common.DayOffInput;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class DayOffInputType : InputObjectType<DayOffInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<DayOffInput> descriptor)
    {
        descriptor.Name("DayOffInput");
        descriptor.Field(d => d.Date).Type<NonNullType<DateTimeType>>();
    }
}
