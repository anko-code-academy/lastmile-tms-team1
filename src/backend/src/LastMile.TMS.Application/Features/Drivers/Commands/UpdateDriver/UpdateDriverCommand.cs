using LastMile.TMS.Application.Features.Drivers.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;

public record UpdateDriverCommand(
    Guid Id,
    string LicenseNumber,
    DateTimeOffset LicenseExpiryDate,
    string? Photo,
    List<ShiftScheduleInput>? ShiftSchedules = null,
    List<DayOffInput>? DaysOff = null) : IRequest<DriverResult>;
