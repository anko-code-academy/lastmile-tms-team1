using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class LabelIntegrationTests
{
    private readonly IntegrationFixture _fx;

    public LabelIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
    }

    private async Task<(Guid parcelId, string trackingNumber)> SeedParcelAsync()
    {
        using var scope = _fx.CreateScope();
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

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var polygon = geometryFactory.CreatePolygon(new[]
        {
            new NetTopologySuite.Geometries.Coordinate(0, 0),
            new NetTopologySuite.Geometries.Coordinate(0.001, 0),
            new NetTopologySuite.Geometries.Coordinate(0.001, 0.001),
            new NetTopologySuite.Geometries.Coordinate(0, 0.001),
            new NetTopologySuite.Geometries.Coordinate(0, 0)
        });

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

        var parcel = Parcel.Create("Test parcel", ServiceType.Express);
        parcel.RecipientAddressId = address.Id;
        parcel.RecipientAddress = address;
        parcel.ShipperAddressId = address.Id;
        parcel.ShipperAddress = address;
        parcel.ZoneId = zone.Id;
        parcel.Zone = zone;
        parcel.ParcelType = ParcelType.Package;
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
        var (parcelId, trackingNumber) = await SeedParcelAsync();

        var response = await _fx.Client.GetAsync($"/labels/{parcelId}/zpl");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(trackingNumber);
        content.Should().Contain("Test User");
        content.Should().Contain("456 Test St");
    }

    [Fact]
    public async Task GetZplLabel_WithInvalidParcelId_ReturnsNotFound()
    {
        var invalidParcelId = Guid.NewGuid();

        var response = await _fx.Client.GetAsync($"/labels/{invalidParcelId}/zpl");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPdfLabel_WithValidParcel_ReturnsOk()
    {
        var (parcelId, trackingNumber) = await SeedParcelAsync();

        var response = await _fx.Client.GetAsync($"/labels/{parcelId}/pdf");

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
        var invalidParcelId = Guid.NewGuid();

        var response = await _fx.Client.GetAsync($"/labels/{invalidParcelId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostBulkLabels_WithMultipleParcelIds_ReturnsPdf()
    {
        var (parcelId1, _) = await SeedParcelAsync();
        var (parcelId2, _) = await SeedParcelAsync();
        var parcelIds = new List<Guid> { parcelId1, parcelId2 };
        var json = JsonSerializer.Serialize(parcelIds);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _fx.Client.PostAsync("/labels/bulk/pdf", content);

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
        var invalidParcelIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var json = JsonSerializer.Serialize(invalidParcelIds);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _fx.Client.PostAsync("/labels/bulk/pdf", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
