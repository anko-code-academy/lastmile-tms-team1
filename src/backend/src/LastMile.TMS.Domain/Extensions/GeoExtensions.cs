using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Extensions;

public static class GeoExtensions
{
    private const double EarthRadiusMeters = 6_371_000;

    public static double HaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    public static double HaversineDistanceMeters(this Point a, Point b)
    {
        return HaversineDistanceMeters(a.Y, a.X, b.Y, b.X);
    }
}
