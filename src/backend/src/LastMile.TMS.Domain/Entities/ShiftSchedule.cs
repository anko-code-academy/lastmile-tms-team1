using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class ShiftSchedule : BaseAuditableEntity
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    // Foreign keys — exactly one must be set (XOR enforced at DB level)
    public Guid? DriverId { get; set; }

    public Guid? DepotId { get; set; }

    // Navigation property
    public Driver Driver { get; set; } = null!;

    // Navigation property
    public Depot Depot { get; set; } = null!;
}