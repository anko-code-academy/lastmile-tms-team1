using MediatR;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand(string Email) : IRequest<Result<bool>>;
