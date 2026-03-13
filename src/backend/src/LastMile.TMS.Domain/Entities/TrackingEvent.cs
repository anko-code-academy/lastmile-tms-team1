using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class TrackingEvent : BaseAuditableEntity
{
    public Guid ParcelId { get; set; }
    public Parcel Parcel { get; set; } = null!;

    public DateTimeOffset Timestamp { get; set; }
    public EventType EventType { get; set; }
    public string? Description { get; set; }

    // Location
    public string? LocationCity { get; set; }
    public string? LocationState { get; set; }
    public string? LocationCountry { get; set; }

    public string? Operator { get; set; }
    public ExceptionReason? DelayReason { get; set; }
}