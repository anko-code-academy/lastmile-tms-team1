using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class DayOff : BaseAuditableEntity
{
    public DateTimeOffset Date { get; set; }

    // Foreign key
    public Guid DriverId { get; set; }

    // Navigation property
    public Driver Driver { get; set; } = null!;
}