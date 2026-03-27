using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class ShiftScheduleTests
{
    [Fact]
    public void NewShiftSchedule_ShouldHaveDefaultValues()
    {
        // Act
        ShiftSchedule schedule = new();

        // Assert
        schedule.DayOfWeek.Should().Be(default(DayOfWeek));
        schedule.OpenTime.Should().Be(default(TimeOnly));
        schedule.CloseTime.Should().Be(default(TimeOnly));
    }

    [Fact]
    public void ShiftSchedule_ShouldAllowSettingProperties()
    {
        // Arrange
        Guid driverId = Guid.CreateVersion7();

        // Act
        ShiftSchedule schedule = new()
        {
            DriverId = driverId,
            DayOfWeek = DayOfWeek.Friday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(14, 0) // Shorter day on Friday
        };

        // Assert
        schedule.DriverId.Should().Be(driverId);
        schedule.DayOfWeek.Should().Be(DayOfWeek.Friday);
        schedule.OpenTime.Should().Be(new TimeOnly(9, 0));
        schedule.CloseTime.Should().Be(new TimeOnly(14, 0));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void ShiftSchedule_ShouldSupportAllDaysOfWeek(DayOfWeek day)
    {
        // Act
        ShiftSchedule schedule = new()
        {
            DayOfWeek = day,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Assert
        schedule.DayOfWeek.Should().Be(day);
    }

    [Fact]
    public void ShiftSchedule_OpenTimeCanBeBeforeCloseTime()
    {
        // Arrange & Act
        ShiftSchedule schedule = new()
        {
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Assert
        schedule.OpenTime.Should().BeBefore(schedule.CloseTime);
    }

    [Fact]
    public void ShiftSchedule_ShouldAllowNullableDriverId()
    {
        // Act
        ShiftSchedule schedule = new()
        {
            DriverId = null,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Assert
        schedule.DriverId.Should().BeNull();
    }

    [Fact]
    public void ShiftSchedule_ShouldAllowNullableDepotId()
    {
        // Act
        ShiftSchedule schedule = new()
        {
            DepotId = null,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Assert
        schedule.DepotId.Should().BeNull();
    }

    [Fact]
    public void ShiftSchedule_ShouldAllowSettingDepotId()
    {
        // Arrange
        Guid depotId = Guid.CreateVersion7();

        // Act
        ShiftSchedule schedule = new()
        {
            DepotId = depotId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Assert
        schedule.DepotId.Should().Be(depotId);
    }

    [Fact]
    public void ShiftSchedule_ShouldAllowSettingBothNullableFk()
    {
        // Arrange
        Guid driverId = Guid.CreateVersion7();
        Guid depotId = Guid.CreateVersion7();

        // Act
        ShiftSchedule driverSchedule = new()
        {
            DriverId = driverId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        ShiftSchedule depotSchedule = new()
        {
            DepotId = depotId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0)
        };

        // Assert
        driverSchedule.DriverId.Should().Be(driverId);
        driverSchedule.DepotId.Should().BeNull();

        depotSchedule.DriverId.Should().BeNull();
        depotSchedule.DepotId.Should().Be(depotId);
    }
}