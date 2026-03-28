using LastMile.TMS.Application.Features.Drivers.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

public record CreateDriverCommand(
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string LicenseNumber,
    DateTimeOffset LicenseExpiryDate,
    string? Photo,
    Guid ZoneId,
    Guid DepotId,
    Guid UserId,
    bool IsActive = true) : IRequest<DriverResult>;
