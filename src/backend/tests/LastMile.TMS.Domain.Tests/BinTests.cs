using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class BinTests
{
    [Theory]
    [InlineData("1", "B", 3, 2, "D1-B-A3-02")]
    [InlineData("1", "A", 1, 1, "D1-A-A1-01")]
    [InlineData("2", "C", 10, 15, "D2-C-A10-15")]
    [InlineData("Depot1", "Z", 5, 9, "DDepot1-Z-A5-09")]
    public void GenerateLabel_ValidInputs_ReturnsCorrectLabel(
        string depotNumber, string zoneLetter, int aisle, int slot, string expected)
    {
        // Act
        var label = Bin.GenerateLabel(depotNumber, zoneLetter, aisle, slot);

        // Assert
        label.Should().Be(expected);
    }

    [Fact]
    public void SetLabel_SetsLabelFromDepotAndZone()
    {
        // Arrange
        var depot = new Depot { Name = "1" };
        var zone = new Zone { Name = "B", Depot = depot };
        var bin = new Bin { Aisle = 3, Slot = 2, Zone = zone };

        // Act
        bin.SetLabel("1", "B");

        // Assert
        bin.Label.Should().Be("D1-B-A3-02");
    }

    [Fact]
    public void SetLabel_SingleLetterZoneName_UsesFirstLetter()
    {
        // Arrange
        var depot = new Depot { Name = "1" };
        var zone = new Zone { Name = "Alpha", Depot = depot };
        var bin = new Bin { Aisle = 1, Slot = 5, Zone = zone };

        // Act - SetLabel takes zoneLetter directly (not zone.Name)
        bin.SetLabel("1", zone.Name[0].ToString());

        // Assert
        bin.Label.Should().Be("D1-A-A1-05");
    }

    [Fact]
    public void GenerateLabel_ZeroAisleOrSlot_PadsWithZero()
    {
        // Act
        var label = Bin.GenerateLabel("1", "A", 0, 0);

        // Assert
        label.Should().Be("D1-A-A0-00");
    }

    [Fact]
    public void Bin_DefaultValues_AreCorrect()
    {
        // Act
        var bin = new Bin();

        // Assert
        bin.Label.Should().BeEmpty();
        bin.IsActive.Should().BeTrue();
        bin.Description.Should().BeNull();
        bin.Aisle.Should().Be(0);
        bin.Slot.Should().Be(0);
        bin.Capacity.Should().Be(0);
    }
}
