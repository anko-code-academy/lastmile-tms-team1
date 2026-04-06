using HotChocolate.Types;
using LastMile.TMS.Application.Features.Bins.Commands.UpdateBin;

namespace LastMile.TMS.Api.GraphQL.Inputs;

public class UpdateBinInput : InputObjectType<UpdateBinCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateBinCommand> descriptor)
    {
        descriptor.Name("UpdateBinCommandInput");

        descriptor.Field(b => b.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(b => b.Description).Type<StringType>();
        descriptor.Field(b => b.Aisle).Type<NonNullType<IntType>>();
        descriptor.Field(b => b.Slot).Type<NonNullType<IntType>>();
        descriptor.Field(b => b.Capacity).Type<NonNullType<IntType>>();
        descriptor.Field(b => b.ZoneId).Type<NonNullType<UuidType>>();
        descriptor.Field(b => b.IsActive).Type<NonNullType<BooleanType>>();
    }
}
