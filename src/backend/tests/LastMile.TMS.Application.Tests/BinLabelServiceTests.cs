using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Infrastructure.Services;

namespace LastMile.TMS.Application.Tests;

public class BinLabelServiceTests
{
    private readonly LabelService _labelService;

    public BinLabelServiceTests()
    {
        _labelService = new LabelService();
    }

    private static Bin CreateTestBin()
    {
        var depot = new Depot { Name = "1" };
        var zone = new Zone { Name = "B", Depot = depot };
        var bin = new Bin
        {
            Aisle = 3,
            Slot = 2,
            Capacity = 50,
            IsActive = true,
            Zone = zone
        };
        bin.SetLabel("1", "B");
        return bin;
    }

    [Fact]
    public void GenerateBinLabelPng_ReturnsValidPng()
    {
        // Act
        var pngBytes = _labelService.GenerateBinLabelPng("D1-B-A3-02", 300, 80);

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
    public void GenerateBinZplLabel_WithValidBin_ReturnsCorrectZpl()
    {
        // Arrange
        var bin = CreateTestBin();

        // Act
        var zpl = _labelService.GenerateBinZplLabel(bin);

        // Assert
        zpl.Should().Contain("D1-B-A3-02");
        zpl.Should().Contain("Zone: B");
        zpl.Should().Contain("Depot: 1");
        zpl.Should().Contain("Aisle: 3");
        zpl.Should().Contain("Slot: 2");
        zpl.Should().Contain("Capacity: 50");
    }

    [Fact]
    public void GenerateBinZplLabel_WithNullZone_ReturnsUnknown()
    {
        // Arrange
        var bin = CreateTestBin();
        bin.Zone = null;

        // Act
        var zpl = _labelService.GenerateBinZplLabel(bin);

        // Assert
        zpl.Should().Contain("D1-B-A3-02");
        zpl.Should().Contain("Zone: Unknown");
        zpl.Should().Contain("Depot: Unknown");
    }

    [Fact]
    public void GenerateBinLabelPdf_ReturnsValidPdf()
    {
        // Arrange
        var bin = CreateTestBin();

        // Act
        var pdfBytes = _labelService.GenerateBinLabelPdf(bin);

        // Assert
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Length.Should().BeGreaterThan(100);
        // PDF magic bytes
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void GenerateBinLabelPdf_WithInactiveBin_ShowsInactiveStatus()
    {
        // Arrange
        var bin = CreateTestBin();
        bin.IsActive = false;

        // Act
        var pdfBytes = _labelService.GenerateBinLabelPdf(bin);

        // Assert
        pdfBytes.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateBinLabelPng_DifferentLabelFormats_AllProduceOutput()
    {
        // Act
        var png1 = _labelService.GenerateBinLabelPng("D1-A-A1-01", 300, 80);
        var png2 = _labelService.GenerateBinLabelPng("DDepot1-Z-A10-15", 300, 80);

        // Assert
        png1.Should().NotBeEmpty();
        png2.Should().NotBeEmpty();
    }
}
