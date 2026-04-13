using System.Net.Http.Json;
using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Polly.RateLimiting;

namespace LastMile.TMS.Infrastructure.Services;

/// <summary>
/// Geocodes addresses using the OpenStreetMap Nominatim API (no API key required).
/// https://nominatim.org/release-docs/latest/api/Overview/
/// </summary>
public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;
    private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/search";

    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LastMileTMS/1.0");
        _logger = logger;
    }

    public async Task<Point?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        var query = Uri.EscapeDataString(address);
        var url = $"{NominatimBaseUrl}?q={query}&format=json&limit=1";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim returned {StatusCode} for address: {Address}", response.StatusCode, address);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<List<NominatimResult>>(cancellationToken);
            if (json is null || json.Count == 0)
            {
                _logger.LogWarning("No geocoding results for address: {Address}", address);
                return null;
            }

            var result = json[0];
            if (!double.TryParse(result.Lat, out var lat) || !double.TryParse(result.Lon, out var lon))
                return null;

            var factory = new GeometryFactory(new PrecisionModel(), 4326);
            var point = factory.CreatePoint(new Coordinate(lon, lat));
            point.SRID = 4326;
            return point;
        }
        catch (RateLimiterRejectedException ex)
        {
            _logger.LogWarning(ex, "Geocoding rate limited for address: {Address}. Queue capacity exceeded.", address);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geocoding failed for address: {Address}", address);
            return null;
        }
    }

    private record NominatimResult(string Lat, string Lon, string? DisplayName);
}
