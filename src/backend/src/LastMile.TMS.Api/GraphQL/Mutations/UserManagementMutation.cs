using LastMile.TMS.Application.Users.Commands.CompletePasswordReset;
using LastMile.TMS.Application.Users.Commands.CreateUser;
using LastMile.TMS.Application.Users.Commands.DeactivateUser;
using LastMile.TMS.Application.Users.Commands.ResetPassword;
using LastMile.TMS.Application.Users.Commands.UpdateUser;
using LastMile.TMS.Application.Users.DTOs;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class UserManagementMutation
{
    public async Task<UserDto> CreateUser(
        string firstName,
        string lastName,
        string email,
        string? phone,
        Guid roleId,
        Guid? zoneId,
        Guid? depotId,
        string password,
        IMediator mediator,
        CancellationToken ct)
    {
        var cmd = new CreateUserCommand(
            firstName,
            lastName,
            email,
            phone,
            roleId,
            zoneId,
            depotId,
            password);

        var result = await mediator.Send(cmd, ct);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to create user");
        }

        return result.Value!;
    }

    public async Task<UserDto> UpdateUser(
        Guid userId,
        string firstName,
        string lastName,
        string email,
        string? phone,
        Guid? roleId,
        Guid? zoneId,
        Guid? depotId,
        IMediator mediator,
        CancellationToken ct)
    {
        var cmd = new UpdateUserCommand(
            userId,
            firstName,
            lastName,
            email,
            phone,
            roleId,
            zoneId,
            depotId);

        var result = await mediator.Send(cmd, ct);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to update user");
        }

        return result.Value!;
    }

    public async Task<UserDto> DeactivateUser(
        Guid userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var cmd = new DeactivateUserCommand(userId);
        var result = await mediator.Send(cmd, ct);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to deactivate user");
        }

        return result.Value!;
    }

    public async Task<bool> ResetPassword(
        string email,
        IMediator mediator,
        CancellationToken ct)
    {
        var cmd = new ResetPasswordCommand(email);
        var result = await mediator.Send(cmd, ct);

        // Always return true for security (don't reveal if user exists)
        return true;
    }

    public async Task<bool> CompletePasswordReset(
        string email,
        string token,
        string newPassword,
        IMediator mediator,
        CancellationToken ct)
    {
        var cmd = new CompletePasswordResetCommand(email, token, newPassword);
        var result = await mediator.Send(cmd, ct);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to reset password");
        }

        return true;
    }
}
