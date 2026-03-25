using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Phone,
    Guid? RoleId,
    Guid? ZoneId,
    Guid? DepotId
) : IRequest<Result<UserDto>>;
