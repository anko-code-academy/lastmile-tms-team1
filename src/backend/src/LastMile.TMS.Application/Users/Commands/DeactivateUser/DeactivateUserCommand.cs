using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest<Result<UserDto>>;
