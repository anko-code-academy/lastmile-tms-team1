using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class ParcelWatcher : BaseAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }

    // Many-to-many relationship with Parcels
    public ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();
}