using LastMile.TMS.Domain.Common;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Entities;

public class Address : BaseAuditableEntity
{
    public string Street1 { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "US";
    public bool IsResidential { get; set; }
    public string? ContactName { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Point? GeoLocation { get; set; }

    // Navigation properties
    public ICollection<Parcel> ShipperParcels { get; set; } = new List<Parcel>();
    public ICollection<Parcel> RecipientParcels { get; set; } = new List<Parcel>();
}
