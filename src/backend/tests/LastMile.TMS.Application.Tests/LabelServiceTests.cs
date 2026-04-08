using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Infrastructure.Services;

namespace LastMile.TMS.Application.Tests;

public class LabelServiceTests
{
    private readonly LabelService _labelService;

    public LabelServiceTests()
    {
        _labelService = new LabelService();
    }

    private static Parcel CreateTestParcel()
    {
        var address = new Address
        {
            Street1 = "123 Main St",
            Street2 = "Apt 4B",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US",
            ContactName = "John Doe",
            CompanyName = "Test Corp"
        };

        var zone = new Zone
        {
            Name = "Zone-A"
        };

        // Use domain factory method to create parcel with proper tracking number
        var parcel = Parcel.Create("Test parcel", ServiceType.Express);
        parcel.RecipientAddress = address;
        parcel.RecipientAddressId = address.Id;
        parcel.Zone = zone;
        parcel.ParcelType = ParcelType.Package;

        return parcel;
    }

    [Fact]
    public void GenerateZplLabel_WithValidParcel_ReturnsCorrectZpl()
    {
        // Arrange
        var parcel = CreateTestParcel();

        // Act
        var zpl = _labelService.GenerateZplLabel(parcel);

        // Assert
        zpl.Should().Contain(parcel.TrackingNumber);
        zpl.Should().Contain("John Doe");
        zpl.Should().Contain("123 Main St");
        zpl.Should().Contain("Springfield");
        zpl.Should().Contain("Zone-A");
        zpl.Should().Contain("Package");
    }

    [Fact]
    public void GenerateZplLabel_WithMissingZone_ReturnsZplWithoutZone()
    {
        // Arrange
        var parcel = CreateTestParcel();
        parcel.Zone = null;

        // Act
        var zpl = _labelService.GenerateZplLabel(parcel);

        // Assert
        zpl.Should().Contain(parcel.TrackingNumber);
        zpl.Should().Contain("Unknown");
    }

    [Fact]
    public void GenerateZplLabel_WithCompanyNameOnly_UsesCompanyName()
    {
        // Arrange
        var parcel = CreateTestParcel();
        parcel.RecipientAddress.ContactName = null;

        // Act
        var zpl = _labelService.GenerateZplLabel(parcel);

        // Assert
        zpl.Should().Contain("Test Corp");
    }

    [Fact]
    public void GenerateZplLabel_WithNullContactAndCompany_UsesNA()
    {
        // Arrange
        var parcel = CreateTestParcel();
        parcel.RecipientAddress.ContactName = null;
        parcel.RecipientAddress.CompanyName = null;

        // Act
        var zpl = _labelService.GenerateZplLabel(parcel);

        // Assert
        zpl.Should().Contain("N/A");
    }

    [Fact]
    public void GenerateBarcodePng_ReturnsValidPng()
    {
        // Arrange
        var trackingNumber = "LMTT1-TEST-123456";

        // Act
        var pngBytes = _labelService.GenerateBarcodePng(trackingNumber, 300, 80);

        // Assert
        pngBytes.Should().NotBeEmpty();
        pngBytes.Length.Should().BeGreaterThan(100);
        // PNG magic bytes
        pngBytes[0].Should().Be(0x89);
        pngBytes[1].Should().Be(0x50);
        pngBytes[2].Should().Be(0x4E);
        pngBytes[3].Should().Be(0x47);
    }

    [Fact]
    public void GenerateQrCodePng_ReturnsValidPng()
    {
        // Arrange
        var trackingNumber = "LMTT1-TEST-123456";

        // Act
        var pngBytes = _labelService.GenerateQrCodePng(trackingNumber, 100);

        // Assert
        pngBytes.Should().NotBeEmpty();
        pngBytes.Length.Should().BeGreaterThan(50);
        // PNG magic bytes
        pngBytes[0].Should().Be(0x89);
        pngBytes[1].Should().Be(0x50);
        pngBytes[2].Should().Be(0x4E);
        pngBytes[3].Should().Be(0x47);
    }

    [Fact]
    public void GeneratePdfLabel_ReturnsValidPdf()
    {
        // Arrange
        var parcel = CreateTestParcel();

        // Act
        var pdfBytes = _labelService.GeneratePdfLabel(parcel);

        // Assert
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Length.Should().BeGreaterThan(100);
        // PDF magic bytes
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void GenerateBulkPdfLabels_WithMultipleParcels_ReturnsPdfWithAllLabels()
    {
        // Arrange
        var parcels = new List<Parcel>
        {
            CreateTestParcel(),
            CreateTestParcel(),
            CreateTestParcel()
        };

        // Act
        var pdfBytes = _labelService.GenerateBulkPdfLabels(parcels);

        // Assert
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Length.Should().BeGreaterThan(100);
        // PDF magic bytes
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void GenerateBulkPdfLabels_WithEmptyList_ReturnsEmptyPdf()
    {
        // Arrange
        var parcels = new List<Parcel>();

        // Act
        var pdfBytes = _labelService.GenerateBulkPdfLabels(parcels);

        // Assert
        pdfBytes.Should().NotBeEmpty();
    }
}
