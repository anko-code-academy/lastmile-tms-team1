using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Tests;

public class RouteTests
{
    [Fact]
    public void Create_ShouldInitializeRouteWithCorrectDefaults()
    {
        // Arrange
        var name = "Route A";
        var plannedStartTime = DateTime.UtcNow.AddDays(1);

        // Act
        var route = new Route
        {
            Name = name,
            PlannedStartTime = plannedStartTime,
            Status = RouteStatus.Draft
        };

        // Assert
        route.Name.Should().Be(name);
        route.Status.Should().Be(RouteStatus.Draft);
        route.PlannedStartTime.Should().Be(plannedStartTime);
        route.ActualStartTime.Should().BeNull();
        route.ActualEndTime.Should().BeNull();
        route.TotalDistanceKm.Should().Be(0);
        route.TotalParcelCount.Should().Be(0);
        route.VehicleId.Should().BeNull();
        route.VehicleJourneys.Should().BeEmpty();
    }

    [Theory]
    [InlineData(RouteStatus.Draft, RouteStatus.InProgress, true)]
    [InlineData(RouteStatus.InProgress, RouteStatus.Completed, true)]
    [InlineData(RouteStatus.Completed, RouteStatus.InProgress, false)]
    [InlineData(RouteStatus.Draft, RouteStatus.Completed, false)]
    [InlineData(RouteStatus.Completed, RouteStatus.Draft, false)]
    [InlineData(RouteStatus.InProgress, RouteStatus.Draft, false)]
    public void CanTransitionTo_ValidatesStatusTransitions(RouteStatus from, RouteStatus to, bool expected)
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = from
        };

        // Act
        var canTransition = route.CanTransitionTo(to);

        // Assert
        canTransition.Should().Be(expected);
    }

    [Fact]
    public void TransitionTo_FromDraftToInProgress_SetsActualStartTime()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Draft
        };

        // Act
        route.TransitionTo(RouteStatus.InProgress);

        // Assert
        route.Status.Should().Be(RouteStatus.InProgress);
        route.ActualStartTime.Should().NotBeNull();
        route.ActualEndTime.Should().BeNull();
    }

    [Fact]
    public void TransitionTo_FromInProgressToCompleted_SetsActualEndTime()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.InProgress
        };

        // Act
        route.TransitionTo(RouteStatus.Completed);

        // Assert
        route.Status.Should().Be(RouteStatus.Completed);
        route.ActualStartTime.Should().BeNull(); // Was never set
        route.ActualEndTime.Should().NotBeNull();
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ThrowsException()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Completed
        };

        // Act
        var act = () => route.TransitionTo(RouteStatus.InProgress);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition from*");
    }

    [Fact]
    public void AssignDriver_OnDraftRoute_SetsDriverId()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Draft
        };

        // Act
        route.AssignDriver(driverId);

        // Assert
        route.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void AssignDriver_OnCompletedRoute_ThrowsException()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Completed
        };

        // Act
        var act = () => route.AssignDriver(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Draft*");
    }

    [Fact]
    public void ReassignDriver_OnDraftRoute_Succeeds()
    {
        // Arrange
        var driverA = Guid.NewGuid();
        var driverB = Guid.NewGuid();
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Draft
        };
        route.AssignDriver(driverA);

        // Act
        route.AssignDriver(driverB);

        // Assert
        route.DriverId.Should().Be(driverB);
    }

    [Fact]
    public void RemoveDriver_SetsDriverIdToNull()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Draft
        };
        route.AssignDriver(Guid.NewGuid());

        // Act
        route.AssignDriver(null);

        // Assert
        route.DriverId.Should().BeNull();
    }

    [Fact]
    public void RecalculateTotals_ShouldCountParcelsAcrossStops()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow
        };

        var stop1 = new RouteStop
        {
            Parcels =
            [
                new Parcel { ServiceType = ServiceType.Standard },
                new Parcel { ServiceType = ServiceType.Standard }
            ]
        };

        var stop2 = new RouteStop
        {
            Parcels =
            [
                new Parcel { ServiceType = ServiceType.Standard },
                new Parcel { ServiceType = ServiceType.Standard },
                new Parcel { ServiceType = ServiceType.Standard }
            ]
        };

        route.RouteStops.Add(stop1);
        route.RouteStops.Add(stop2);

        // Act
        route.RecalculateTotals();

        // Assert
        route.TotalParcelCount.Should().Be(5);
    }

    [Fact]
    public void RecalculateTotals_WithNoStops_ShouldSetZeroCounts()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow
        };

        // Act
        route.RecalculateTotals();

        // Assert
        route.TotalParcelCount.Should().Be(0);
    }

    [Fact]
    public void RouteWithZone_SetsZoneId()
    {
        // Arrange
        var zoneId = Guid.NewGuid();

        // Act
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            ZoneId = zoneId
        };

        // Assert
        route.ZoneId.Should().Be(zoneId);
    }

    [Fact]
    public void RecalculateDistance_DepotToStopsToDepot_CalculatesHaversineDistance()
    {
        // Depot at (0, 0), stops at ~(0.01, 0.01) and ~(0.02, 0.02)
        var depotGeo = new Point(0, 0) { SRID = 4326 };
        var route = new Route
        {
            Name = "Distance Test Route",
            PlannedStartTime = DateTime.UtcNow,
            RouteStops =
            [
                new RouteStop
                {
                    SequenceNumber = 1,
                    Street1 = "Stop 1",
                    GeoLocation = new Point(0.01, 0.01) { SRID = 4326 }
                },
                new RouteStop
                {
                    SequenceNumber = 2,
                    Street1 = "Stop 2",
                    GeoLocation = new Point(0.02, 0.02) { SRID = 4326 }
                }
            ]
        };

        route.RecalculateDistance(depotGeo);

        route.TotalDistanceKm.Should().BeGreaterThan(0);
        // depot->stop1 + stop1->stop2 ≈ 2 * haversine(0,0, 0.01,0.01) + haversine(0.01,0.01, 0.02,0.02)
        // Should be roughly in the range of a few km (each ~1.57 km)
        route.TotalDistanceKm.Should().BeLessThan(20);
    }

    [Fact]
    public void RecalculateDistance_NoStops_SetsZeroDistance()
    {
        var depotGeo = new Point(0, 0) { SRID = 4326 };
        var route = new Route
        {
            Name = "Empty Route",
            PlannedStartTime = DateTime.UtcNow
        };

        route.RecalculateDistance(depotGeo);

        route.TotalDistanceKm.Should().Be(0);
    }

    [Fact]
    public void RecalculateDistance_NullDepot_SetsZeroDistance()
    {
        var route = new Route
        {
            Name = "No Depot Route",
            PlannedStartTime = DateTime.UtcNow,
            RouteStops =
            [
                new RouteStop
                {
                    SequenceNumber = 1,
                    Street1 = "Stop 1",
                    GeoLocation = new Point(0.01, 0.01) { SRID = 4326 }
                }
            ]
        };

        route.RecalculateDistance(null);

        route.TotalDistanceKm.Should().Be(0);
    }

    [Fact]
    public void RecalculateDistance_StopsWithoutGeo_SkipsThem()
    {
        var depotGeo = new Point(0, 0) { SRID = 4326 };
        var route = new Route
        {
            Name = "Mixed Stops Route",
            PlannedStartTime = DateTime.UtcNow,
            RouteStops =
            [
                new RouteStop
                {
                    SequenceNumber = 1,
                    Street1 = "Stop With Geo",
                    GeoLocation = new Point(0.01, 0.01) { SRID = 4326 }
                },
                new RouteStop
                {
                    SequenceNumber = 2,
                    Street1 = "Stop Without Geo",
                    GeoLocation = null
                }
            ]
        };

        route.RecalculateDistance(depotGeo);

        // Should only count depot->stop1 (skip stop2 since it has no geo)
        route.TotalDistanceKm.Should().BeGreaterThan(0);
        route.TotalDistanceKm.Should().BeLessThan(5);
    }
}
