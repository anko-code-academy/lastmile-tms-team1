using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class LabelIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public LabelIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
    {
        _factory = new IntegrationTestWebApplicationFactory(postgreSqlFixture);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<(Guid parcelId, string trackingNumber)> SeedParcelAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LastMile.TMS.Persistence.AppDbContext>();

        var address = new Address
        {
            Street1 = "456 Test St",
            City = "Testville",
            State = "TS",
            PostalCode = "12345",
            CountryCode = "US",
            ContactName = "Test User"
        };

        // Create a proper polygon for zone boundary (not a point)
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var polygon = geometryFactory.CreatePolygon(new[]
        {
            new NetTopologySuite.Geometries.Coordinate(0, 0),
            new NetTopologySuite.Geometries.Coordinate(0.001, 0),
            new NetTopologySuite.Geometries.Coordinate(0.001, 0.001),
            new NetTopologySuite.Geometries.Coordinate(0, 0.001),
            new NetTopologySuite.Geometries.Coordinate(0, 0)
        });

        // Create depot first, then zone that references it
        var depot = new Depot
        {
            Name = "Test Depot",
            IsActive = true,
            Address = address,
            AddressId = address.Id
        };

        var zone = new Zone
        {
            Name = "Test-Zone",
            IsActive = true,
            Depot = depot,
            BoundaryGeometry = polygon
        };

        context.Addresses.Add(address);
        context.Depots.Add(depot);
        await context.SaveChangesAsync();

        // Use domain factory method to create parcel with proper tracking number
        var parcel = Parcel.Create("Test parcel", ServiceType.Express);
        parcel.RecipientAddressId = address.Id;
        parcel.RecipientAddress = address;
        parcel.ShipperAddressId = address.Id;
        parcel.ShipperAddress = address;
        parcel.ZoneId = zone.Id;
        parcel.Zone = zone;
        parcel.ParcelType = "Standard";
        parcel.Weight = 1.5m;
        parcel.WeightUnit = WeightUnit.Kg;
        parcel.Length = 10m;
        parcel.Width = 10m;
        parcel.Height = 10m;
        parcel.DimensionUnit = DimensionUnit.Cm;
        parcel.DeclaredValue = 100m;
        parcel.Currency = "USD";

        context.Zones.Add(zone);
        context.Parcels.Add(parcel);
        await context.SaveChangesAsync();

        return (parcel.Id, parcel.TrackingNumber);
    }

    [Fact]
    public async Task GetZplLabel_WithValidParcel_ReturnsOk()
    {
        // Arrange
        var (parcelId, trackingNumber) = await SeedParcelAsync();

        // Act
        var response = await _client.GetAsync($"/labels/{parcelId}/zpl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(trackingNumber);
        content.Should().Contain("Test User");
        content.Should().Contain("456 Test St");
    }

    [Fact]
    public async Task GetZplLabel_WithInvalidParcelId_ReturnsNotFound()
    {
        // Arrange
        var invalidParcelId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/labels/{invalidParcelId}/zpl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPdfLabel_WithValidParcel_ReturnsOk()
    {
        // Arrange
        var (parcelId, trackingNumber) = await SeedParcelAsync();

        // Act
        var response = await _client.GetAsync($"/labels/{parcelId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        var header = Encoding.ASCII.GetString(content, 0, 5);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task GetPdfLabel_WithInvalidParcelId_ReturnsNotFound()
    {
        // Arrange
        var invalidParcelId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/labels/{invalidParcelId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostBulkLabels_WithMultipleParcelIds_ReturnsPdf()
    {
        // Arrange
        var (parcelId1, _) = await SeedParcelAsync();
        var (parcelId2, _) = await SeedParcelAsync();
        var parcelIds = new List<Guid> { parcelId1, parcelId2 };
        var json = JsonSerializer.Serialize(parcelIds);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/labels/bulk/pdf", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        responseBytes.Should().NotBeEmpty();
        var header = Encoding.ASCII.GetString(responseBytes, 0, 5);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task PostBulkLabels_WithInvalidParcelIds_ReturnsNotFound()
    {
        // Arrange
        var invalidParcelIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var json = JsonSerializer.Serialize(invalidParcelIds);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/labels/bulk/pdf", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
