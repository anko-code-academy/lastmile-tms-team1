using MediatR;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Features.Users.Commands.ActivateUser;

public record ActivateUserCommand(Guid UserId) : IRequest<Result<UserDto>>;