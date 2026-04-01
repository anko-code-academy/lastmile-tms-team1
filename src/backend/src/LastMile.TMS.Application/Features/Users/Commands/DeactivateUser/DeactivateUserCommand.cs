using MediatR;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest<Result<UserDto>>;
