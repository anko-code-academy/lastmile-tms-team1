using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Users.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    UserStatus Status,
    string? RoleName,
    Guid? RoleId,
    Guid? ZoneId,
    Guid? DepotId,
    DateTimeOffset CreatedAt
);
