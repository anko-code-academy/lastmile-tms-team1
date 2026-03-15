using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class VehicleTests
{
    [Fact]
    public void Create_ShouldInitializeVehicleWithCorrectDefaults()
    {
        // Act
        var vehicle = Vehicle.Create("ABC123", VehicleType.Van, 50, 1000m);

        // Assert
        vehicle.RegistrationPlate.Should().Be("ABC123");
        vehicle.Type.Should().Be(VehicleType.Van);
        vehicle.ParcelCapacity.Should().Be(50);
        vehicle.WeightCapacityKg.Should().Be(1000m);
        vehicle.Status.Should().Be(VehicleStatus.Available);
    }

    [Fact]
    public void Create_ShouldUpperCaseRegistrationPlate()
    {
        // Act
        var vehicle = Vehicle.Create("abc123", VehicleType.Car, 5, 500m);

        // Assert
        vehicle.RegistrationPlate.Should().Be("ABC123");
    }

    [Fact]
    public void TransitionTo_ValidTransition_ShouldSucceed()
    {
        // Arrange
        var vehicle = Vehicle.Create("ABC123", VehicleType.Bike, 2, 50m);

        // Act & Assert
        vehicle.TransitionTo(VehicleStatus.InUse);
        vehicle.Status.Should().Be(VehicleStatus.InUse);
    }

    [Theory]
    [InlineData(VehicleStatus.Available, true)]
    [InlineData(VehicleStatus.InUse, false)]  // Same status not allowed
    [InlineData(VehicleStatus.Maintenance, true)]
    [InlineData(VehicleStatus.Retired, false)]
    public void CanTransitionFromInUse(VehicleStatus targetStatus, bool expected)
    {
        // Arrange
        var vehicle = Vehicle.Create("ABC123", VehicleType.Van, 50, 1000m);
        vehicle.Status = VehicleStatus.InUse;

        // Act
        var result = vehicle.CanTransitionTo(targetStatus);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ShouldThrow()
    {
        // Arrange
        var vehicle = Vehicle.Create("ABC123", VehicleType.Van, 50, 1000m);
        vehicle.Status = VehicleStatus.Retired;

        // Act
        var act = () => vehicle.TransitionTo(VehicleStatus.InUse);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition*");
    }

    [Fact]
    public void CanTransitionFromRetired_ShouldNotAllowAnyTransitions()
    {
        // Arrange
        var vehicle = Vehicle.Create("ABC123", VehicleType.Van, 50, 1000m);
        vehicle.Status = VehicleStatus.Retired;

        // Act & Assert - Retired is terminal state
        vehicle.CanTransitionTo(VehicleStatus.Available).Should().BeFalse();
        vehicle.CanTransitionTo(VehicleStatus.InUse).Should().BeFalse();
        vehicle.CanTransitionTo(VehicleStatus.Maintenance).Should().BeFalse();
        vehicle.CanTransitionTo(VehicleStatus.Retired).Should().BeFalse(); // Same status not allowed
    }

    [Theory]
    [InlineData(VehicleStatus.Available)]
    [InlineData(VehicleStatus.InUse)]
    [InlineData(VehicleStatus.Maintenance)]
    [InlineData(VehicleStatus.Retired)]
    public void CanTransitionTo_SameStatus_ShouldReturnFalse(VehicleStatus status)
    {
        // Arrange
        var vehicle = Vehicle.Create("ABC123", VehicleType.Van, 50, 1000m);
        vehicle.Status = status;

        // Act
        var result = vehicle.CanTransitionTo(status);

        // Assert
        result.Should().BeFalse();
    }
}
