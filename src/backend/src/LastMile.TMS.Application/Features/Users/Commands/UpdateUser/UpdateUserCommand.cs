using MediatR;
using LastMile.TMS.Application.Features.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Guid? RoleId,
    Guid? ZoneId,
    Guid? DepotId
) : IRequest<Result<UserDto>>;
