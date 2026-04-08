using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Features.Routes;

internal static class DriverDayOffValidator
{
    public static void EnsureAvailableForDate(Driver driver, DateTime routeDate)
    {
        var dateOnly = DateOnly.FromDateTime(routeDate);
        if (driver.DaysOff.Any(d => DateOnly.FromDateTime(d.Date.DateTime) == dateOnly))
        {
            throw new InvalidOperationException("Cannot assign driver who has a day off on the route date.");
        }
    }
}
