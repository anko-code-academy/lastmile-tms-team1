using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class ParcelContentItem : BaseAuditableEntity
{
    public Guid ParcelId { get; set; }
    public Parcel Parcel { get; set; } = null!;

    // Format: XXXX.XX
    public string? HsCode { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitValue { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Weight { get; set; }
    public WeightUnit WeightUnit { get; set; }

    // ISO 3166-1 alpha-2
    public string CountryOfOrigin { get; set; } = "US";
}