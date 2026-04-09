using FluentAssertions;
using LastMile.TMS.Domain.Extensions;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Tests;

public class GeoExtensionsTests
{
    // Tolerance for floating-point distance comparisons (meters)
    private const double Tolerance = 10.0;

    [Fact]
    public void HaversineDistanceMeters_SamePoint_ReturnsZero()
    {
        var result = GeoExtensions.HaversineDistanceMeters(40.7128, -74.0060, 40.7128, -74.0060);

        result.Should().BeApproximately(0, 0.1);
    }

    [Fact]
    public void HaversineDistanceMeters_ShortCityBlock_About100Meters()
    {
        // Two points on the same avenue, roughly one building apart
        // 40.7128, -74.0060 → 40.7137, -74.0060 (~100m north)
        var result = GeoExtensions.HaversineDistanceMeters(40.7128, -74.0060, 40.7137, -74.0060);

        result.Should().BeApproximately(100, Tolerance);
    }

    [Fact]
    public void HaversineDistanceMeters_AcrossNeighborhood_About2Km()
    {
        // Across a city district
        // 40.7128, -74.0060 → 40.7308, -74.0060 (~2km north)
        var result = GeoExtensions.HaversineDistanceMeters(40.7128, -74.0060, 40.7308, -74.0060);

        result.Should().BeApproximately(2000, 50);
    }

    [Fact]
    public void HaversineDistanceMeters_CrossTown_About10Km()
    {
        // Cross-town distance across a metro area
        // 40.7128, -74.0060 → 40.7831, -73.9440 (~10km)
        var result = GeoExtensions.HaversineDistanceMeters(40.7128, -74.0060, 40.7831, -73.9440);

        result.Should().BeApproximately(10000, 700);
    }

    [Fact]
    public void HaversineDistanceMeters_PointExtension_MatchesRawCoordinates()
    {
        var a = new Point(-74.0060, 40.7128) { SRID = 4326 };
        var b = new Point(-73.9854, 40.7484) { SRID = 4326 };

        var extensionResult = a.HaversineDistanceMeters(b);
        var rawResult = GeoExtensions.HaversineDistanceMeters(a.Y, a.X, b.Y, b.X);

        extensionResult.Should().BeApproximately(rawResult, 0.01);
    }
}
