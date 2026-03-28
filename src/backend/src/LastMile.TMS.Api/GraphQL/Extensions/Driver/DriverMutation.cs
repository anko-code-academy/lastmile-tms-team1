using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;
using LastMile.TMS.Application.Features.Drivers.Commands.DeleteDriver;
using LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;
using LastMile.TMS.Application.Features.Drivers.Common;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.Driver;

[ExtendObjectType(typeof(Mutation))]
public class DriverMutation
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<DriverResult> CreateDriver(CreateDriverCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<DriverResult> UpdateDriver(UpdateDriverCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
    public async Task<bool> DeleteDriver(Guid id, [Service] IMediator mediator)
        => await mediator.Send(new DeleteDriverCommand(id));
}
