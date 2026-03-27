using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Driver : BaseAuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;
    public DateTimeOffset LicenseExpiryDate { get; set; }

    public string? Photo { get; set; }

    // Foreign keys
    public Guid ZoneId { get; set; }
    public Guid DepotId { get; set; }
    public Guid UserId { get; set; } // FK to User (for mobile app login)

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ShiftSchedule> ShiftSchedules { get; set; } = new List<ShiftSchedule>();
    public ICollection<DayOff> DaysOff { get; set; } = [];

    // Navigation properties
    public Zone Zone { get; set; } = null!;
    public Depot Depot { get; set; } = null!;

    // Navigation properties - commented until User entity is available
    // public User User { get; set; } = null!;
}
