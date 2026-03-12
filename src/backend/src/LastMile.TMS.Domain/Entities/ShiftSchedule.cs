using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class ShiftSchedule : BaseAuditableEntity
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    // Foreign key
    public Guid DriverId { get; set; }

    // Navigation property
    public Driver Driver { get; set; } = null!;
}