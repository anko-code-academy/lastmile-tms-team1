using System.Net;
using FluentAssertions;
using LastMile.TMS.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Services;

public class NominatimGeocodingServiceTests : IDisposable
{
    private readonly MockHttpMessageHandler _handler;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;
    private readonly NominatimGeocodingService _sut;

    public NominatimGeocodingServiceTests()
    {
        _handler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_handler);
        _logger = Substitute.For<ILogger<NominatimGeocodingService>>();
        _sut = new NominatimGeocodingService(_httpClient, _logger);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handler.Dispose();
    }

    [Fact]
    public async Task GeocodeAsync_ValidResponse_ReturnsPoint()
    {
        // Arrange
        _handler.SetResponse("[{\"lat\":\"39.7817\",\"lon\":\"-89.6501\",\"display_name\":\"Springfield, IL\"}]");

        // Act
        var result = await _sut.GeocodeAsync("Springfield, IL");

        // Assert
        result.Should().NotBeNull();
        result!.SRID.Should().Be(4326);
        result.X.Should().BeApproximately(-89.6501, 0.0001); // lon = X
        result.Y.Should().BeApproximately(39.7817, 0.0001);  // lat = Y
    }

    [Fact]
    public async Task GeocodeAsync_EmptyAddress_ReturnsNull()
    {
        // Act
        var result = await _sut.GeocodeAsync("");

        // Assert
        result.Should().BeNull();
        _handler.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_HttpError_ReturnsNull()
    {
        // Arrange
        _handler.SetResponse(HttpStatusCode.InternalServerError);

        // Act
        var result = await _sut.GeocodeAsync("some address");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_EmptyResults_ReturnsNull()
    {
        // Arrange
        _handler.SetResponse("[]");

        // Act
        var result = await _sut.GeocodeAsync("nonexistent address");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_InvalidCoordinates_ReturnsNull()
    {
        // Arrange
        _handler.SetResponse("[{\"lat\":\"not-a-number\",\"lon\":\"also-not\",\"display_name\":\"test\"}]");

        // Act
        var result = await _sut.GeocodeAsync("address");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_NetworkException_ReturnsNull()
    {
        // Arrange
        _handler.SetException(new HttpRequestException("Connection refused"));

        // Act
        var result = await _sut.GeocodeAsync("address");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_SetsUserAgentHeader()
    {
        // Arrange
        _handler.SetResponse("[]");

        // Act
        await _sut.GeocodeAsync("some address");

        // Assert
        _handler.LastRequest.Should().NotBeNull();
        _handler.LastRequest!.Headers.UserAgent.ToString().Should().Contain("LastMileTMS/1.0");
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage? _response;
        private Exception? _exception;

        public HttpRequestMessage? LastRequest { get; private set; }

        public void SetResponse(string json)
        {
            _response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
            _exception = null;
        }

        public void SetResponse(HttpStatusCode statusCode)
        {
            _response = new HttpResponseMessage(statusCode);
            _exception = null;
        }

        public void SetException(Exception exception)
        {
            _exception = exception;
            _response = null;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            if (_exception is not null)
                return Task.FromException<HttpResponseMessage>(_exception);

            return Task.FromResult(_response!);
        }
    }
}
