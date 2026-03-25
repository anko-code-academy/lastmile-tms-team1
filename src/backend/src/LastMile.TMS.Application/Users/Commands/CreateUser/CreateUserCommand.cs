using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Guid RoleId,
    Guid? ZoneId,
    Guid? DepotId,
    string Password
) : IRequest<Result<UserDto>>;
