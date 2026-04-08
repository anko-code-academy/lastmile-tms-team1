using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Services;

/// <summary>
/// Calculates estimated delivery dates based on service type,
/// skipping weekends and US federal holidays.
/// </summary>
public interface IDeliveryDateCalculator
{
    DateTimeOffset CalculateEstimatedDeliveryDate(ServiceType serviceType, DateTimeOffset fromDate);
}

public class DeliveryDateCalculator : IDeliveryDateCalculator
{
    // US federal holidays for near-future planning
    private static readonly HashSet<DateTime> HolidaySet =
    [
        new(2026, 1, 1),   new(2026, 1, 19),  new(2026, 2, 16),  new(2026, 5, 25),
        new(2026, 6, 19),  new(2026, 7, 3),   new(2026, 9, 7),   new(2026, 10, 12),
        new(2026, 11, 11), new(2026, 11, 26), new(2026, 12, 25),
        new(2027, 1, 1),   new(2027, 1, 18),  new(2027, 2, 15),  new(2027, 5, 31),
        new(2027, 6, 18),  new(2027, 7, 5),   new(2027, 9, 6),   new(2027, 10, 11),
        new(2027, 11, 11), new(2027, 11, 25), new(2027, 12, 24),
    ];

    public DateTimeOffset CalculateEstimatedDeliveryDate(ServiceType serviceType, DateTimeOffset fromDate)
    {
        var businessDays = serviceType switch
        {
            ServiceType.Overnight => 1,
            ServiceType.Express => 2,
            ServiceType.Standard => 5,
            ServiceType.Economy => 10,
            _ => 5
        };

        return AddBusinessDays(fromDate, businessDays);
    }

    private static DateTimeOffset AddBusinessDays(DateTimeOffset start, int businessDays)
    {
        var current = start.Date;
        var added = 0;

        while (added < businessDays)
        {
            current = current.AddDays(1);
            var day = current.DayOfWeek;
            if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                continue;
            if (HolidaySet.Contains(current.Date))
                continue;
            added++;
        }

        return new DateTimeOffset(current, start.Offset);
    }
}
