using HotChocolate.Types;
using LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateDepotInput : InputObjectType<UpdateDepotCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateDepotCommand> descriptor)
    {
        descriptor.Name("UpdateDepotInput");

        descriptor.Field(d => d.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(d => d.Name).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.IsActive).Type<NonNullType<BooleanType>>();
    }
}
