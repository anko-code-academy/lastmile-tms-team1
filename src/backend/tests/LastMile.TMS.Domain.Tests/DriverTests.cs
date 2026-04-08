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
        driver.LicenseNumber.Should().BeEmpty();
        driver.ShiftSchedules.Should().BeEmpty();
        driver.DaysOff.Should().BeEmpty();
    }

    [Fact]
    public void Driver_ShouldAllowSettingProperties()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var licenseExpiry = DateTimeOffset.UtcNow.AddYears(1);

        // Act
        Driver driver = new()
        {
            UserId = userId,
            LicenseNumber = "DL123456",
            LicenseExpiryDate = licenseExpiry,
            Photo = "https://example.com/photo.jpg"
        };

        // Assert
        driver.UserId.Should().Be(userId);
        driver.LicenseNumber.Should().Be("DL123456");
        driver.LicenseExpiryDate.Should().Be(licenseExpiry);
        driver.Photo.Should().Be("https://example.com/photo.jpg");
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
    public void Driver_Photo_ShouldBeNullable()
    {
        // Arrange & Act
        Driver driver = new();

        // Assert
        driver.Photo.Should().BeNull();
    }
}