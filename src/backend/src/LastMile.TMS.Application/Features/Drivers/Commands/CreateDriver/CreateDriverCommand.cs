using LastMile.TMS.Application.Features.Drivers.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

public record CreateDriverCommand(
    string Email,
    string LicenseNumber,
    DateTimeOffset LicenseExpiryDate,
    string? Photo,
    List<ShiftScheduleInput>? ShiftSchedules = null,
    List<DayOffInput>? DaysOff = null) : IRequest<DriverResult>
{
    internal Guid? ResolvedUserId { get; set; }
}
