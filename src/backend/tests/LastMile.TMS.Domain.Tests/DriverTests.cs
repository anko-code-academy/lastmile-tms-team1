using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class DriverTests
{
    [Fact]
    public void NewDriver_ShouldHaveDefaultValues()
    {
        // Act
        Driver driver = new();

        // Assert
        driver.FirstName.Should().BeEmpty();
        driver.LastName.Should().BeEmpty();
        driver.Phone.Should().BeEmpty();
        driver.Email.Should().BeEmpty();
        driver.LicenseNumber.Should().BeEmpty();
        driver.IsActive.Should().BeTrue();
        driver.ShiftSchedules.Should().BeEmpty();
        driver.DaysOff.Should().BeEmpty();
    }

    [Fact]
    public void Driver_ShouldAllowSettingProperties()
    {
        // Arrange
        Guid zoneId = Guid.CreateVersion7();
        Guid depotId = Guid.CreateVersion7();
        Guid userId = Guid.CreateVersion7();
        var licenseExpiry = DateTimeOffset.UtcNow.AddYears(1);

        // Act
        Driver driver = new()
        {
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1234567890",
            Email = "john.doe@example.com",
            LicenseNumber = "DL123456",
            LicenseExpiryDate = licenseExpiry,
            Photo = "https://example.com/photo.jpg",
            ZoneId = zoneId,
            DepotId = depotId,
            UserId = userId,
            IsActive = false
        };

        // Assert
        driver.FirstName.Should().Be("John");
        driver.LastName.Should().Be("Doe");
        driver.Phone.Should().Be("+1234567890");
        driver.Email.Should().Be("john.doe@example.com");
        driver.LicenseNumber.Should().Be("DL123456");
        driver.LicenseExpiryDate.Should().Be(licenseExpiry);
        driver.Photo.Should().Be("https://example.com/photo.jpg");
        driver.ZoneId.Should().Be(zoneId);
        driver.DepotId.Should().Be(depotId);
        driver.UserId.Should().Be(userId);
        driver.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Driver_ShouldHaveNavigationCollectionsInitialized()
    {
        // Arrange & Act
        Driver driver = new();

        // Assert
        driver.ShiftSchedules.Should().NotBeNull();
        driver.ShiftSchedules.Should().BeEmpty();
        driver.DaysOff.Should().NotBeNull();
        driver.DaysOff.Should().BeEmpty();
    }

    [Fact]
    public void Driver_ShouldAllowAddingShiftSchedules()
    {
        // Arrange
        Driver driver = new();
        var schedule = new ShiftSchedule
        {
            DriverId = driver.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Act
        driver.ShiftSchedules.Add(schedule);

        // Assert
        driver.ShiftSchedules.Should().HaveCount(1);
        driver.ShiftSchedules.Should().Contain(schedule);
    }

    [Fact]
    public void Driver_ShouldAllowAddingDaysOff()
    {
        // Arrange
        Driver driver = new();
        var dayOff = new DayOff
        {
            DriverId = driver.Id,
            Date = DateTimeOffset.UtcNow.AddDays(5)
        };

        // Act
        driver.DaysOff.Add(dayOff);

        // Assert
        driver.DaysOff.Should().HaveCount(1);
        driver.DaysOff.Should().Contain(dayOff);
    }

    [Fact]
    public void Driver_ShouldAllowSettingZoneNavigation()
    {
        // Arrange
        Driver driver = new();
        Zone zone = new() { Name = "Zone 1" };

        // Act
        driver.Zone = zone;

        // Assert
        driver.Zone.Should().Be(zone);
        driver.ZoneId.Should().Be(zone.Id);
    }

    [Fact]
    public void Driver_ShouldAllowSettingDepotNavigation()
    {
        // Arrange
        Driver driver = new();
        Depot depot = new() { Name = "Main Depot" };

        // Act
        driver.Depot = depot;

        // Assert
        driver.Depot.Should().Be(depot);
        driver.DepotId.Should().Be(depot.Id);
    }

    [Fact]
    public void Driver_Photo_ShouldBeNullable()
    {
        // Arrange & Act
        Driver driver = new();

        // Assert
        driver.Photo.Should().BeNull();
    }

    [Fact]
    public void Driver_ZoneAndDepot_ShouldBeInitiallyNull()
    {
        // Arrange & Act
        Driver driver = new();

        // Assert
        driver.Zone.Should().BeNull();
        driver.Depot.Should().BeNull();
    }
}