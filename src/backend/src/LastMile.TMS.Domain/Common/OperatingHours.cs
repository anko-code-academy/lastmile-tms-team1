using System.Diagnostics.CodeAnalysis;

namespace LastMile.TMS.Domain.Common;

public record DailyOperatingHours(DayOfWeek DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);

public record OperatingHours
{
    public required IReadOnlyCollection<DailyOperatingHours> Schedule { get; init; }

    public TimeOnly? GetOpenTime(DayOfWeek day) =>
        Schedule.FirstOrDefault(x => x.DayOfWeek == day)?.OpenTime;

    public TimeOnly? GetCloseTime(DayOfWeek day) =>
        Schedule.FirstOrDefault(x => x.DayOfWeek == day)?.CloseTime;

    public bool IsOpenOn(DayOfWeek day) =>
        Schedule.Any(x => x.DayOfWeek == day);

    public static OperatingHours Create([DisallowNull] params DailyOperatingHours[] schedule) =>
        new() { Schedule = schedule.ToList().AsReadOnly() };

    public static OperatingHours CreateWeekdays(TimeOnly open, TimeOnly close) =>
        Create(
            new DailyOperatingHours(DayOfWeek.Monday, open, close),
            new DailyOperatingHours(DayOfWeek.Tuesday, open, close),
            new DailyOperatingHours(DayOfWeek.Wednesday, open, close),
            new DailyOperatingHours(DayOfWeek.Thursday, open, close),
            new DailyOperatingHours(DayOfWeek.Friday, open, close));
}
