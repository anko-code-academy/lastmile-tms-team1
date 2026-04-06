using HotChocolate.Types;
using LastMile.TMS.Application.Features.Bins.Commands.CreateBin;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class CreateBinInput : InputObjectType<CreateBinCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateBinCommand> descriptor)
    {
        descriptor.Name("CreateBinCommandInput");

        descriptor.Field(b => b.Description).Type<StringType>();
        descriptor.Field(b => b.Aisle).Type<NonNullType<IntType>>();
        descriptor.Field(b => b.Slot).Type<NonNullType<IntType>>();
        descriptor.Field(b => b.Capacity).Type<NonNullType<IntType>>();
        descriptor.Field(b => b.ZoneId).Type<NonNullType<UuidType>>();
        descriptor.Field(b => b.IsActive).DefaultValue(true);
    }
}
