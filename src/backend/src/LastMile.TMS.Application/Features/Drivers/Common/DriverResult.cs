namespace LastMile.TMS.Application.Features.Drivers.Common;

public record DriverResult(
    Guid Id,
    string LicenseNumber,
    DateTimeOffset LicenseExpiryDate,
    string? Photo,
    Guid UserId,
    DateTimeOffset CreatedAt,
    List<ShiftScheduleResult>? ShiftSchedules = null,
    List<DayOffResult>? DaysOff = null);

public record ShiftScheduleResult(DayOfWeek DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);

public record DayOffResult(DateTimeOffset Date);
