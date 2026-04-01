using HotChocolate.Authorization;
using LastMile.TMS.Api.GraphQL;
using LastMile.TMS.Application.Features.Users.Commands.ActivateUser;
using LastMile.TMS.Application.Features.Users.Commands.CompletePasswordReset;
using LastMile.TMS.Application.Features.Users.Commands.CreateUser;
using LastMile.TMS.Application.Features.Users.Commands.DeactivateUser;
using LastMile.TMS.Application.Features.Users.Commands.ResetPassword;
using LastMile.TMS.Application.Features.Users.Commands.UpdateUser;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Extensions.UserManagement;

[ExtendObjectType(typeof(Mutation))]
public class UserManagementMutation
{
    [Authorize(Roles = [Role.RoleNames.Admin])]
    public async Task<UserDto> CreateUser(
        [Service] IMediator mediator,
        string firstName,
        string lastName,
        string email,
        string? phone,
        Guid roleId,
        Guid? zoneId,
        Guid? depotId,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new CreateUserCommand(firstName, lastName, email, phone, roleId, zoneId, depotId, password),
            cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error ?? "Failed to create user");

        return result.Value;
    }

    [Authorize(Roles = [Role.RoleNames.Admin])]
    public async Task<UserDto> UpdateUser(
        [Service] IMediator mediator,
        Guid userId,
        string firstName,
        string lastName,
        string email,
        string? phone,
           Guid? roleId,
        Guid? zoneId,
        Guid? depotId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new UpdateUserCommand(userId, firstName, lastName, email, phone, roleId, zoneId, depotId),
            cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error ?? "Failed to update user");

        return result.Value;
    }

    [Authorize(Roles = [Role.RoleNames.Admin])]
    public async Task<UserDto> DeactivateUser(
        [Service] IMediator mediator,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeactivateUserCommand(userId), cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error ?? "Failed to deactivate user");

        return result.Value;
    }

    [Authorize(Roles = [Role.RoleNames.Admin])]
    public async Task<UserDto> ActivateUser(
        [Service] IMediator mediator,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ActivateUserCommand(userId), cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error ?? "Failed to activate user");

        return result.Value;
    }

    public async Task<bool> ResetPassword(
        [Service] IMediator mediator,
        string email,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new ResetPasswordCommand(email), cancellationToken);

        // Always return true for security (don't reveal if user exists)
        return true;
    }

    public async Task<bool> CompletePasswordReset(
        [Service] IMediator mediator,
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new CompletePasswordResetCommand(email, token, newPassword),
            cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error ?? "Failed to reset password");

        return true;
    }
}
