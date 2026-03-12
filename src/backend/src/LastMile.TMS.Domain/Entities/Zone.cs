using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Zone : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();
}