namespace LastMile.TMS.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Domain.Enums.UserStatus Status,
    Guid? RoleId,
    string? RoleName,
    Guid? ZoneId,
    string? ZoneName,
    Guid? DepotId,
    string? DepotName,
    DateTimeOffset CreatedAt
);
