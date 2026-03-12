using LastMile.TMS.Domain.Common;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace LastMile.TMS.Domain.Entities;

public class Zone : BaseAuditableEntity
{
    private static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public string Name { get; set; } = string.Empty;

    public Geometry BoundaryGeometry { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Foreign key
    public Guid DepotId { get; set; }

    // Navigation properties
    public Depot Depot { get; set; } = null!;
    public ICollection<Driver> Drivers { get; set; } = [];

    public void SetBoundaryFromGeoJson(string geoJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(geoJson);

        GeoJsonReader reader = new(GeometryFactory, new JsonSerializerSettings());
        Geometry geometry = reader.Read<Geometry>(geoJson);

        if (geometry is not Polygon polygon)
        {
            throw new ArgumentException("GeoJSON must represent a Polygon.", nameof(geoJson));
        }

        BoundaryGeometry = polygon;
    }
}
