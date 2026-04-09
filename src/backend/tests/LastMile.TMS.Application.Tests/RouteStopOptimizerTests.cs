using FluentAssertions;
using LastMile.TMS.Application.Features.Routes.Services;

namespace LastMile.TMS.Application.Tests;

public class RouteStopOptimizerTests
{
    private readonly RouteStopOptimizer _optimizer = new();

    // Helper to create stops at known coordinates
    private static RouteStopGeoInfo Stop(Guid id, double lat, double lon, int originalIndex = 0)
        => new(id, lat, lon, originalIndex);

    [Fact]
    public void OptimizeStopOrder_ThreeCollinearStops_ReturnsNearestNeighborOrder()
    {
        // Three stops in a line: B is between A and C
        // Depot at A. Should visit A nearest, then B, then C.
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        // Depot at lat 0, lon 0
        // A at lat 0, lon 0.01 (~1.1km from depot)
        // B at lat 0, lon 0.02 (~2.2km from depot)
        // C at lat 0, lon 0.04 (~4.4km from depot)
        var stops = new List<RouteStopGeoInfo>
        {
            Stop(c, 0, 0.04, 0),
            Stop(a, 0, 0.01, 1),
            Stop(b, 0, 0.02, 2),
        };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 0, depotLongitude: 0);

        result.Should().Equal([a, b, c]);
    }

    [Fact]
    public void OptimizeStopOrder_WithDepot_StartsFromNearestStop()
    {
        var near = Guid.NewGuid();
        var far = Guid.NewGuid();

        var stops = new List<RouteStopGeoInfo>
        {
            Stop(far, 0, 0.1, 0),
            Stop(near, 0, 0.001, 1),
        };

        // Depot at 0,0 — 'near' should be visited first
        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 0, depotLongitude: 0);

        result[0].Should().Be(near);
    }

    [Fact]
    public void OptimizeStopOrder_SingleStop_ReturnsSingleStop()
    {
        var id = Guid.NewGuid();
        var stops = new List<RouteStopGeoInfo> { Stop(id, 40.0, -74.0, 0) };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 40.0, depotLongitude: -74.0);

        result.Should().Equal([id]);
    }

    [Fact]
    public void OptimizeStopOrder_TwoStops_ReturnsBothStops()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var stops = new List<RouteStopGeoInfo>
        {
            Stop(a, 0, 0.01, 0),
            Stop(b, 0, 0.02, 1),
        };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 0, depotLongitude: 0);

        result.Should().HaveCount(2);
        result.Should().Contain([a, b]);
    }

    [Fact]
    public void OptimizeStopOrder_EmptyList_ReturnsEmpty()
    {
        var result = _optimizer.OptimizeStopOrder([], depotLatitude: 0, depotLongitude: 0);

        result.Should().BeEmpty();
    }

    [Fact]
    public void OptimizeStopOrder_AllStopsMissingCoordinates_ReturnsOriginalOrder()
    {
        // All stops have NaN coordinates (simulating null GeoLocation)
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var stops = new List<RouteStopGeoInfo>
        {
            Stop(a, double.NaN, double.NaN, 0),
            Stop(b, double.NaN, double.NaN, 1),
            Stop(c, double.NaN, double.NaN, 2),
        };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 0, depotLongitude: 0);

        // Should return original order since none can be optimized
        result.Should().Equal([a, b, c]);
    }

    [Fact]
    public void OptimizeStopOrder_SomeStopsMissingCoordinates_OptimizableFirst_NullsAppended()
    {
        var valid1 = Guid.NewGuid();
        var valid2 = Guid.NewGuid();
        var nullStop = Guid.NewGuid();
        var stops = new List<RouteStopGeoInfo>
        {
            Stop(nullStop, double.NaN, double.NaN, 0),
            Stop(valid1, 0, 0.01, 1),
            Stop(valid2, 0, 0.02, 2),
        };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 0, depotLongitude: 0);

        // Valid stops should come first (optimized), null at end
        result.Should().HaveCount(3);
        result[2].Should().Be(nullStop);
    }

    [Fact]
    public void OptimizeStopOrder_CrossingPath_2OptImprovesOrder()
    {
        // Create 4 stops that form a crossing path when visited in naive order:
        //
        //   A(0,0)          C(0,2)
        //        \          /
        //         X (cross)
        //        /          \
        //   B(1,0)          D(1,2)
        //
        // NN from depot at (0,0) might go A→B→C→D (crossing)
        // 2-opt should uncross to A→C→D→B or A→B→D→C
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var d = Guid.NewGuid();

        var stops = new List<RouteStopGeoInfo>
        {
            Stop(a, 0, 0, 0),
            Stop(c, 0, 2, 1),
            Stop(b, 1, 0, 2),
            Stop(d, 1, 2, 3),
        };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: -0.5, depotLongitude: -0.5);

        // Verify the result is better than the naive A→C→B→D crossing order
        var naiveDistance = TourDistance(stops, [a, c, b, d]);
        var optimizedDistance = TourDistanceFromDepot(stops, result, -0.5, -0.5);

        optimizedDistance.Should().BeLessThanOrEqualTo(naiveDistance + 0.01);
    }

    [Fact]
    public void OptimizeStopOrder_AlreadyOptimal_NoDegradation()
    {
        // Stops already in optimal order along a line
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var d = Guid.NewGuid();

        var stops = new List<RouteStopGeoInfo>
        {
            Stop(a, 0, 0.01, 0),
            Stop(b, 0, 0.02, 1),
            Stop(c, 0, 0.03, 2),
            Stop(d, 0, 0.04, 3),
        };

        var result = _optimizer.OptimizeStopOrder(stops, depotLatitude: 0, depotLongitude: 0);

        var originalDistance = TourDistanceFromDepot(stops, [a, b, c, d], 0, 0);
        var resultDistance = TourDistanceFromDepot(stops, result, 0, 0);

        resultDistance.Should().BeApproximately(originalDistance, 1.0);
    }

    // Helper: compute tour distance between stops in given order (no depot)
    private static double TourDistance(List<RouteStopGeoInfo> stops, List<Guid> order)
    {
        double total = 0;
        for (int i = 0; i < order.Count - 1; i++)
        {
            var from = stops.First(s => s.StopId == order[i]);
            var to = stops.First(s => s.StopId == order[i + 1]);
            total += Distance(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
        }
        return total;
    }

    // Helper: compute tour distance from depot through stops
    private static double TourDistanceFromDepot(List<RouteStopGeoInfo> stops, List<Guid> order, double depotLat, double depotLon)
    {
        if (order.Count == 0) return 0;

        var first = stops.First(s => s.StopId == order[0]);
        double total = Distance(depotLat, depotLon, first.Latitude, first.Longitude);

        for (int i = 0; i < order.Count - 1; i++)
        {
            var from = stops.First(s => s.StopId == order[i]);
            var to = stops.First(s => s.StopId == order[i + 1]);
            total += Distance(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
        }
        return total;
    }

    private static double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6_371_000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
