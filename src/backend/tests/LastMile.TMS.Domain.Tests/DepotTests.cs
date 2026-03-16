using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class DepotTests
{
    [Fact]
    public void NewDepot_ShouldHaveDefaultValues()
    {
        // Act
        Depot depot = new();

        // Assert
        depot.Name.Should().BeEmpty();
        depot.IsActive.Should().BeTrue();
        depot.Zones.Should().BeEmpty();
    }

    [Fact]
    public void Depot_ShouldAllowSettingProperties()
    {
        // Arrange
        Depot depot = new()
        {
            Name = "Downtown Depot",
            IsActive = false
        };

        // Assert
        depot.Name.Should().Be("Downtown Depot");
        depot.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Depot_ShouldHaveZonesCollectionInitialized()
    {
        // Arrange & Act
        Depot depot = new();

        // Assert
        depot.Zones.Should().NotBeNull();
        depot.Zones.Should().BeEmpty();
    }

    [Fact]
    public void Depot_ShouldAllowAddingZones()
    {
        // Arrange
        Depot depot = new();
        Zone zone = new() { DepotId = depot.Id };

        // Act
        depot.Zones.Add(zone);

        // Assert
        depot.Zones.Should().HaveCount(1);
        depot.Zones.Should().Contain(zone);
    }
}
