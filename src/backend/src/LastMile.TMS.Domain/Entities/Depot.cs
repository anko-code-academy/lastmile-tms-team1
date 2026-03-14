using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Depot : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public Address? Address { get; set; }
    public Guid? AddressId { get; set; }

    public OperatingHours OperatingHours { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Zone> Zones { get; set; } = [];
}
