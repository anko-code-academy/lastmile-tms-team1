namespace LastMile.TMS.Application.Features.Drivers.Common;

public record DriverResult(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string LicenseNumber,
    DateTimeOffset LicenseExpiryDate,
    string? Photo,
    Guid ZoneId,
    Guid DepotId,
    Guid UserId,
    bool IsActive,
    DateTimeOffset CreatedAt);
