using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Depot : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public Address Address { get; set; } = null!;
    public Guid AddressId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Zone> Zones { get; set; } = [];
    public ICollection<ShiftSchedule> ShiftSchedules { get; set; } = new List<ShiftSchedule>();
}