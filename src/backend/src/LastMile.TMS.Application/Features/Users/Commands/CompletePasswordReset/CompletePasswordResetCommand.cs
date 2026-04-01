using LastMile.TMS.Domain.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Users.Commands.CompletePasswordReset;

public record CompletePasswordResetCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Result<bool>>;
