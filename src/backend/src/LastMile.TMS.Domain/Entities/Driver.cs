using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Driver : BaseAuditableEntity
{
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTimeOffset LicenseExpiryDate { get; set; }

    public string? Photo { get; set; }

    // FK to User (for mobile app login)
    public Guid UserId { get; set; }

    // Navigation properties
    public ICollection<ShiftSchedule> ShiftSchedules { get; set; } = new List<ShiftSchedule>();
    public ICollection<DayOff> DaysOff { get; set; } = [];
    public ICollection<Route> Routes { get; set; } = [];

    // User that this driver profile belongs to
    public User User { get; set; } = null!;
}
