using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Users.DTOs;

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
    string? ZoneName,
    Guid? DepotId,
    string? DepotName,
    DateTimeOffset CreatedAt
);
