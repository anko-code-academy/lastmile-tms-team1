using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

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
            Status = RouteStatus.Planned
        };

        // Assert
        route.Name.Should().Be(name);
        route.Status.Should().Be(RouteStatus.Planned);
        route.PlannedStartTime.Should().Be(plannedStartTime);
        route.ActualStartTime.Should().BeNull();
        route.ActualEndTime.Should().BeNull();
        route.TotalDistanceKm.Should().Be(0);
        route.TotalParcelCount.Should().Be(0);
        route.VehicleId.Should().BeNull();
        route.VehicleJourneys.Should().BeEmpty();
    }

    [Theory]
    [InlineData(RouteStatus.Planned, RouteStatus.InProgress, true)]
    [InlineData(RouteStatus.InProgress, RouteStatus.Completed, true)]
    [InlineData(RouteStatus.Planned, RouteStatus.Cancelled, true)]
    [InlineData(RouteStatus.InProgress, RouteStatus.Cancelled, true)]
    [InlineData(RouteStatus.Completed, RouteStatus.InProgress, false)]
    [InlineData(RouteStatus.Completed, RouteStatus.Cancelled, false)]
    [InlineData(RouteStatus.Cancelled, RouteStatus.Completed, false)]
    [InlineData(RouteStatus.Cancelled, RouteStatus.InProgress, false)]
    [InlineData(RouteStatus.Planned, RouteStatus.Completed, false)]
    [InlineData(RouteStatus.Completed, RouteStatus.Planned, false)]
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
    public void TransitionTo_FromPlannedToInProgress_SetsActualStartTime()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Planned
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
    public void TransitionTo_FromPlannedToCancelled_Succeeds()
    {
        // Arrange
        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Planned
        };

        // Act
        route.TransitionTo(RouteStatus.Cancelled);

        // Assert
        route.Status.Should().Be(RouteStatus.Cancelled);
        route.ActualStartTime.Should().BeNull();
        route.ActualEndTime.Should().BeNull();
    }
}
