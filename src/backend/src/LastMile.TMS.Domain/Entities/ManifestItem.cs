using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class ManifestItem : BaseAuditableEntity
{
    public Guid ManifestId { get; private set; }
    public Manifest Manifest { get; set; } = null!;
    public string TrackingNumber { get; private set; } = string.Empty;
    public Guid? ParcelId { get; private set; }
    public Parcel? Parcel { get; set; }
    public ManifestItemStatus Status { get; private set; }

    public static ManifestItem Create(Guid manifestId, string trackingNumber)
    {
        return new ManifestItem
        {
            ManifestId = manifestId,
            TrackingNumber = trackingNumber,
            Status = ManifestItemStatus.Expected
        };
    }

    public void MarkReceived(Guid parcelId)
    {
        if (Status != ManifestItemStatus.Expected)
            throw new InvalidOperationException($"Item is already {Status}");

        Status = ManifestItemStatus.Received;
        ParcelId = parcelId;
    }

    public void MarkUnexpected(Guid parcelId)
    {
        if (Status != ManifestItemStatus.Expected)
            throw new InvalidOperationException($"Item is already {Status}");

        Status = ManifestItemStatus.Unexpected;
        ParcelId = parcelId;
    }

    public void MarkMissing()
    {
        if (Status != ManifestItemStatus.Expected)
            throw new InvalidOperationException($"Item is already {Status}");

        Status = ManifestItemStatus.Missing;
    }
}
