using FluentAssertions;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Tests;

public class OperatingHoursTests
{
    [Fact]
    public void Create_WithValidSchedule_ShouldSetSchedule()
    {
        // Arrange
        DailyOperatingHours schedule = new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));

        // Act
        OperatingHours operatingHours = OperatingHours.Create(schedule);

        // Assert
        operatingHours.Schedule.Should().HaveCount(1);
        operatingHours.Schedule.First().DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void CreateWeekdays_ShouldCreateScheduleForMondayToFriday()
    {
        // Arrange
        TimeOnly openTime = new(8, 0);
        TimeOnly closeTime = new(18, 0);

        // Act
        OperatingHours operatingHours = OperatingHours.CreateWeekdays(openTime, closeTime);

        // Assert
        operatingHours.Schedule.Should().HaveCount(5);
        operatingHours.IsOpenOn(DayOfWeek.Monday).Should().BeTrue();
        operatingHours.IsOpenOn(DayOfWeek.Tuesday).Should().BeTrue();
        operatingHours.IsOpenOn(DayOfWeek.Wednesday).Should().BeTrue();
        operatingHours.IsOpenOn(DayOfWeek.Thursday).Should().BeTrue();
        operatingHours.IsOpenOn(DayOfWeek.Friday).Should().BeTrue();
        operatingHours.IsOpenOn(DayOfWeek.Saturday).Should().BeFalse();
        operatingHours.IsOpenOn(DayOfWeek.Sunday).Should().BeFalse();
    }

    [Fact]
    public void GetOpenTime_ExistingDay_ShouldReturnOpenTime()
    {
        // Arrange
        OperatingHours operatingHours = OperatingHours.Create(
            new DailyOperatingHours(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            new DailyOperatingHours(DayOfWeek.Friday, new TimeOnly(8, 0), new TimeOnly(16, 0)));

        // Act
        TimeOnly? mondayOpen = operatingHours.GetOpenTime(DayOfWeek.Monday);
        TimeOnly? fridayOpen = operatingHours.GetOpenTime(DayOfWeek.Friday);

        // Assert
        mondayOpen.Should().Be(new TimeOnly(9, 0));
        fridayOpen.Should().Be(new TimeOnly(8, 0));
    }

    [Fact]
    public void GetOpenTime_NonExistingDay_ShouldReturnNull()
    {
        // Arrange
        OperatingHours operatingHours = OperatingHours.Create(
            new DailyOperatingHours(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)));

        // Act
        TimeOnly? saturdayOpen = operatingHours.GetOpenTime(DayOfWeek.Saturday);

        // Assert
        saturdayOpen.Should().BeNull();
    }

    [Fact]
    public void GetCloseTime_ExistingDay_ShouldReturnCloseTime()
    {
        // Arrange
        OperatingHours operatingHours = OperatingHours.Create(
            new DailyOperatingHours(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)));

        // Act
        TimeOnly? mondayClose = operatingHours.GetCloseTime(DayOfWeek.Monday);

        // Assert
        mondayClose.Should().Be(new TimeOnly(17, 0));
    }

    [Fact]
    public void IsOpenOn_ExistingDay_ShouldReturnTrue()
    {
        // Arrange
        OperatingHours operatingHours = OperatingHours.Create(
            new DailyOperatingHours(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)));

        // Act & Assert
        operatingHours.IsOpenOn(DayOfWeek.Monday).Should().BeTrue();
    }

    [Fact]
    public void IsOpenOn_NonExistingDay_ShouldReturnFalse()
    {
        // Arrange
        OperatingHours operatingHours = OperatingHours.Create(
            new DailyOperatingHours(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)));

        // Act & Assert
        operatingHours.IsOpenOn(DayOfWeek.Saturday).Should().BeFalse();
        operatingHours.IsOpenOn(DayOfWeek.Sunday).Should().BeFalse();
    }
}
