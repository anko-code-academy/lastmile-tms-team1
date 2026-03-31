namespace LastMile.TMS.Application.Features.Drivers.Common;

public record ShiftScheduleInput(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime);
