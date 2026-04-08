using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class RouteStopTests
{
    [Fact]
    public void Create_ShouldInitializeWithCorrectDefaults()
    {
        // Arrange & Act
        var stop = new RouteStop
        {
            SequenceNumber = 1,
            RouteId = Guid.NewGuid(),
            Street1 = "123 Main St"
        };

        // Assert
        stop.SequenceNumber.Should().Be(1);
        stop.Status.Should().Be(RouteStopStatus.Pending);
        stop.ArrivalTime.Should().BeNull();
        stop.DepartureTime.Should().BeNull();
        stop.EstimatedServiceMinutes.Should().Be(0);
        stop.AccessInstructions.Should().BeNull();
        stop.Street1.Should().Be("123 Main St");
        stop.GeoLocation.Should().BeNull();
        stop.Parcels.Should().BeEmpty();
    }

    [Theory]
    [InlineData(RouteStopStatus.Pending, RouteStopStatus.Arrived, true)]
    [InlineData(RouteStopStatus.Arrived, RouteStopStatus.Completed, true)]
    [InlineData(RouteStopStatus.Pending, RouteStopStatus.Skipped, true)]
    [InlineData(RouteStopStatus.Pending, RouteStopStatus.Completed, false)]
    [InlineData(RouteStopStatus.Pending, RouteStopStatus.Pending, false)]
    [InlineData(RouteStopStatus.Arrived, RouteStopStatus.Arrived, false)]
    [InlineData(RouteStopStatus.Completed, RouteStopStatus.Arrived, false)]
    [InlineData(RouteStopStatus.Completed, RouteStopStatus.Pending, false)]
    [InlineData(RouteStopStatus.Skipped, RouteStopStatus.Arrived, false)]
    [InlineData(RouteStopStatus.Skipped, RouteStopStatus.Pending, false)]
    [InlineData(RouteStopStatus.Arrived, RouteStopStatus.Skipped, false)]
    public void CanTransitionTo_ValidatesStopStatusTransitions(RouteStopStatus from, RouteStopStatus to, bool expected)
    {
        // Arrange
        var stop = new RouteStop { Status = from };

        // Act
        var canTransition = stop.CanTransitionTo(to);

        // Assert
        canTransition.Should().Be(expected);
    }

    [Fact]
    public void TransitionTo_FromPendingToArrived_SetsArrivalTime()
    {
        // Arrange
        var stop = new RouteStop { Status = RouteStopStatus.Pending };

        // Act
        stop.TransitionTo(RouteStopStatus.Arrived);

        // Assert
        stop.Status.Should().Be(RouteStopStatus.Arrived);
        stop.ArrivalTime.Should().NotBeNull();
        stop.DepartureTime.Should().BeNull();
    }

    [Fact]
    public void TransitionTo_FromArrivedToCompleted_SetsDepartureTime()
    {
        // Arrange
        var stop = new RouteStop { Status = RouteStopStatus.Arrived };

        // Act
        stop.TransitionTo(RouteStopStatus.Completed);

        // Assert
        stop.Status.Should().Be(RouteStopStatus.Completed);
        stop.DepartureTime.Should().NotBeNull();
    }

    [Fact]
    public void TransitionTo_FromPendingToSkipped_SetsDepartureTime()
    {
        // Arrange
        var stop = new RouteStop { Status = RouteStopStatus.Pending };

        // Act
        stop.TransitionTo(RouteStopStatus.Skipped);

        // Assert
        stop.Status.Should().Be(RouteStopStatus.Skipped);
        stop.DepartureTime.Should().NotBeNull();
        stop.ArrivalTime.Should().BeNull();
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ThrowsException()
    {
        // Arrange
        var stop = new RouteStop { Status = RouteStopStatus.Completed };

        // Act
        var act = () => stop.TransitionTo(RouteStopStatus.Arrived);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition from*");
    }
}
