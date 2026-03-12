using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class DayOffTests
{
    [Fact]
    public void NewDayOff_ShouldHaveDefaultValues()
    {
        // Act
        DayOff dayOff = new();

        // Assert
        dayOff.Date.Should().Be(default(DateTimeOffset));
    }

    [Fact]
    public void DayOff_ShouldAllowSettingProperties()
    {
        // Arrange
        Guid driverId = Guid.CreateVersion7();
        var date = new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.Zero);

        // Act
        DayOff dayOff = new()
        {
            DriverId = driverId,
            Date = date
        };

        // Assert
        dayOff.DriverId.Should().Be(driverId);
        dayOff.Date.Should().Be(date);
    }

    [Fact]
    public void DayOff_ShouldAllowSettingDriverNavigation()
    {
        // Arrange
        DayOff dayOff = new();
        Driver driver = new();

        // Act
        dayOff.Driver = driver;

        // Assert
        dayOff.Driver.Should().Be(driver);
        dayOff.DriverId.Should().Be(driver.Id);
    }

    [Fact]
    public void DayOff_ShouldSupportFutureDates()
    {
        // Arrange
        var futureDate = DateTimeOffset.UtcNow.AddMonths(1);

        // Act
        DayOff dayOff = new()
        {
            Date = futureDate
        };

        // Assert
        dayOff.Date.Should().Be(futureDate);
    }

    [Fact]
    public void DayOff_ShouldSupportPastDates()
    {
        // Arrange
        var pastDate = DateTimeOffset.UtcNow.AddMonths(-1);

        // Act
        DayOff dayOff = new()
        {
            Date = pastDate
        };

        // Assert
        dayOff.Date.Should().Be(pastDate);
    }

    [Fact]
    public void DayOff_ShouldSupportDatesWithTimeZone()
    {
        // Arrange
        var dateWithTimeZone = new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.FromHours(-5));

        // Act
        DayOff dayOff = new()
        {
            Date = dateWithTimeZone
        };

        // Assert
        dayOff.Date.Offset.Should().Be(TimeSpan.FromHours(-5));
    }
}