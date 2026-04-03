using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class ParcelAuditLog : BaseAuditableEntity
{
    public Guid ParcelId { get; private set; }
    public string PropertyName { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;

    public Parcel Parcel { get; set; } = null!;

    public static ParcelAuditLog Create(Guid parcelId, string propertyName, string? oldValue, string? newValue, string changedBy)
    {
        return new ParcelAuditLog
        {
            ParcelId = parcelId,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy
        };
    }
}
