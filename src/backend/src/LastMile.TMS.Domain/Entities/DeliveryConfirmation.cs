using LastMile.TMS.Domain.Common;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Entities;

public class DeliveryConfirmation : BaseAuditableEntity
{
    public Guid ParcelId { get; set; }
    public Parcel Parcel { get; set; } = null!;

    public string? ReceivedBy { get; set; }
    public string? DeliveryLocation { get; set; }
    public string? SignatureImage { get; set; }
    public string? Photo { get; set; }
    public DateTimeOffset DeliveredAt { get; set; }

    // GPS coordinates at delivery
    public Point? DeliveryLocationCoords { get; set; }
}