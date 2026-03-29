using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class VehicleJourneyTests
{
    [Fact]
    public void Create_ShouldInitializeVehicleJourneyWithCorrectDefaults()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        // Act
        var journey = new VehicleJourney
        {
            RouteId = routeId,
            VehicleId = vehicleId,
            StartTime = startTime,
            StartMileageKm = 1000m,
            EndMileageKm = 1050m
        };

        // Assert
        journey.RouteId.Should().Be(routeId);
        journey.VehicleId.Should().Be(vehicleId);
        journey.StartTime.Should().Be(startTime);
        journey.EndTime.Should().BeNull();
        journey.StartMileageKm.Should().Be(1000m);
        journey.EndMileageKm.Should().Be(1050m);
    }

    [Fact]
    public void DistanceKm_ShouldCalculateCorrectly_WhenEndMileageIsGreater()
    {
        // Arrange
        var journey = new VehicleJourney
        {
            StartMileageKm = 1000m,
            EndMileageKm = 1050m
        };

        // Act
        var distance = journey.DistanceKm;

        // Assert
        distance.Should().Be(50m);
    }

    [Fact]
    public void DistanceKm_ShouldReturnZero_WhenEndMileageIsLessThanStart()
    {
        // Arrange
        var journey = new VehicleJourney
        {
            StartMileageKm = 1050m,
            EndMileageKm = 1000m
        };

        // Act
        var distance = journey.DistanceKm;

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceKm_ShouldReturnZero_WhenEndMileageIsZero()
    {
        // Arrange
        var journey = new VehicleJourney
        {
            StartMileageKm = 1000m,
            EndMileageKm = 0
        };

        // Act
        var distance = journey.DistanceKm;

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceKm_ShouldReturnZero_WhenEndMileageIsNotSet()
    {
        // Arrange
        var journey = new VehicleJourney
        {
            StartMileageKm = 1000m,
            EndMileageKm = 0
        };

        // Act
        var distance = journey.DistanceKm;

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceKm_ShouldCalculateCorrectly_ForLargeDistance()
    {
        // Arrange
        var journey = new VehicleJourney
        {
            StartMileageKm = 0m,
            EndMileageKm = 500.5m
        };

        // Act
        var distance = journey.DistanceKm;

        // Assert
        distance.Should().Be(500.5m);
    }
}
