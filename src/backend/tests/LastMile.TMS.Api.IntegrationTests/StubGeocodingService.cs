using LastMile.TMS.Application.Common.Interfaces;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Api.IntegrationTests;

public class StubGeocodingService : IGeocodingService
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

    // Returns a valid point near Springfield, IL (approx -89.5, 39.8)
    public Task<Point?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Task.FromResult<Point?>(null);

        var point = Factory.CreatePoint(new Coordinate(-89.5, 39.8));
        point.SRID = 4326;
        return Task.FromResult<Point?>(point);
    }
}
