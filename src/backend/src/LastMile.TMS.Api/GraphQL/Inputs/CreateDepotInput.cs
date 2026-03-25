using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CreateDepotInput : InputObjectType<CreateDepotCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateDepotCommand> descriptor)
    {
        descriptor.Name("CreateDepotCommandInput");

        descriptor.Field(d => d.Name).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.IsActive).DefaultValue(true);
        descriptor.Field(d => d.OperatingHours).Type<ListType<DailyOperatingHoursInputType>>();
    }
}
