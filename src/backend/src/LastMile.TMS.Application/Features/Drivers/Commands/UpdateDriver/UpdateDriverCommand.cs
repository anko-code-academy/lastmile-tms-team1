using LastMile.TMS.Application.Features.Drivers.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;

public record UpdateDriverCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string LicenseNumber,
    DateTimeOffset LicenseExpiryDate,
    string? Photo,
    Guid ZoneId,
    Guid DepotId,
    bool IsActive,
    List<ShiftScheduleInput>? ShiftSchedules = null,
    List<DayOffInput>? DaysOff = null) : IRequest<DriverResult>;
