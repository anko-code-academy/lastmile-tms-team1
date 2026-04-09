using LastMile.TMS.Domain.Extensions;

namespace LastMile.TMS.Application.Features.Routes.Services;

public interface IRouteStopOptimizer
{
    List<Guid> OptimizeStopOrder(IReadOnlyList<RouteStopGeoInfo> stops, double depotLatitude, double depotLongitude);
}

public class RouteStopOptimizer : IRouteStopOptimizer
{
    public List<Guid> OptimizeStopOrder(IReadOnlyList<RouteStopGeoInfo> stops, double depotLatitude, double depotLongitude)
    {
        if (stops.Count == 0) return [];

        // Separate valid stops from those with missing coordinates
        var validStops = stops.Where(s => !double.IsNaN(s.Latitude) && !double.IsNaN(s.Longitude)).ToList();
        var nullStops = stops.Where(s => double.IsNaN(s.Latitude) || double.IsNaN(s.Longitude))
            .OrderBy(s => s.OriginalIndex).ToList();

        if (validStops.Count == 0)
        {
            return stops.OrderBy(s => s.OriginalIndex).Select(s => s.StopId).ToList();
        }

        // Build distance matrix
        var n = validStops.Count;
        var matrix = new double[n, n];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
        {
            if (i != j)
                matrix[i, j] = GeoExtensions.HaversineDistanceMeters(
                    validStops[i].Latitude, validStops[i].Longitude,
                    validStops[j].Latitude, validStops[j].Longitude);
        }

        // Distance from depot to each stop
        var depotDistances = new double[n];
        for (var i = 0; i < n; i++)
        {
            depotDistances[i] = GeoExtensions.HaversineDistanceMeters(
                depotLatitude, depotLongitude,
                validStops[i].Latitude, validStops[i].Longitude);
        }

        // Nearest-neighbor starting from stop closest to depot
        var order = NearestNeighbor(matrix, depotDistances, n);

        // 2-opt refinement
        order = TwoOpt(matrix, order, n);

        // Build result
        var result = order.Select(i => validStops[i].StopId).ToList();

        // Append stops with missing coordinates at the end
        result.AddRange(nullStops.Select(s => s.StopId));

        return result;
    }

    private static List<int> NearestNeighbor(double[,] matrix, double[] depotDistances, int n)
    {
        // Start from the stop closest to depot
        var start = 0;
        var minDist = depotDistances[0];
        for (var i = 1; i < n; i++)
        {
            if (depotDistances[i] < minDist)
            {
                minDist = depotDistances[i];
                start = i;
            }
        }

        var visited = new bool[n];
        var order = new List<int>(n) { start };
        visited[start] = true;
        var current = start;

        for (var step = 1; step < n; step++)
        {
            var nearest = -1;
            var nearestDist = double.MaxValue;

            for (var j = 0; j < n; j++)
            {
                if (!visited[j] && matrix[current, j] < nearestDist)
                {
                    nearestDist = matrix[current, j];
                    nearest = j;
                }
            }

            order.Add(nearest);
            visited[nearest] = true;
            current = nearest;
        }

        return order;
    }

    private static List<int> TwoOpt(double[,] matrix, List<int> order, int n)
    {
        var maxPasses = Math.Max(10, n);
        var improved = true;
        var passes = 0;

        while (improved && passes < maxPasses)
        {
            improved = false;
            passes++;

            for (var i = 0; i < n - 1; i++)
            {
                for (var j = i + 2; j < n; j++)
                {
                    // Current edges: (i, i+1) and (j, j+1 or wrap)
                    var a = order[i];
                    var b = order[i + 1];
                    var c = order[j];
                    var d = j + 1 < n ? order[j + 1] : -1;

                    var currentDist = matrix[a, b];
                    var newDist = matrix[a, c];

                    if (d >= 0)
                    {
                        currentDist += matrix[c, d];
                        newDist += matrix[b, d];
                    }

                    if (newDist < currentDist - 0.001)
                    {
                        // Reverse the segment between i+1 and j
                        order.Reverse(i + 1, j - i);
                        improved = true;
                    }
                }
            }
        }

        return order;
    }
}
