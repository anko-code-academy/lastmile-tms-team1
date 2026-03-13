using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class ParcelTests
{
    [Fact]
    public void GenerateTrackingNumber_ShouldGenerateUniqueTrackingNumbers()
    {
        // Act
        var trackingNumber1 = Parcel.GenerateTrackingNumber();
        var trackingNumber2 = Parcel.GenerateTrackingNumber();

        // Assert
        trackingNumber1.Should().NotBeNullOrEmpty();
        trackingNumber2.Should().NotBeNullOrEmpty();
        trackingNumber1.Should().NotBe(trackingNumber2);
    }

    [Fact]
    public void GenerateTrackingNumber_ShouldFollowExpectedFormat()
    {
        // Act
        var trackingNumber = Parcel.GenerateTrackingNumber();

        // Assert
        trackingNumber.Should().StartWith("LMTT1-");
        trackingNumber.Length.Should().Be(21); // LMTT1-YYYYMMDD-XXXXXX
    }

    [Fact]
    public void Create_ShouldInitializeParcelWithCorrectDefaults()
    {
        // Act
        var parcel = Parcel.Create("Test description", ServiceType.Express);

        // Assert
        parcel.TrackingNumber.Should().StartWith("LMTT1-");
        parcel.Description.Should().Be("Test description");
        parcel.ServiceType.Should().Be(ServiceType.Express);
        parcel.Status.Should().Be(ParcelStatus.Registered);
        parcel.DeliveryAttempts.Should().Be(0);
    }

    [Fact]
    public void TransitionTo_ValidTransition_ShouldSucceed()
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = ParcelStatus.Registered;

        // Act & Assert
        parcel.TransitionTo(ParcelStatus.ReceivedAtDepot);
        parcel.Status.Should().Be(ParcelStatus.ReceivedAtDepot);
    }

    [Fact]
    public void TransitionTo_ValidTransition_ToDelivered_ShouldSetActualDeliveryDate()
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = ParcelStatus.OutForDelivery;

        // Act
        parcel.TransitionTo(ParcelStatus.Delivered);

        // Assert
        parcel.Status.Should().Be(ParcelStatus.Delivered);
        parcel.ActualDeliveryDate.Should().NotBeNull();
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ShouldThrow()
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = ParcelStatus.Registered;

        // Act
        var act = () => parcel.TransitionTo(ParcelStatus.Delivered);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition*");
    }

    [Theory]
    [InlineData(ParcelStatus.Registered, false)]
    [InlineData(ParcelStatus.ReceivedAtDepot, true)]
    [InlineData(ParcelStatus.Cancelled, true)]
    [InlineData(ParcelStatus.Exception, true)]
    public void CanTransitionFromRegistered(ParcelStatus targetStatus, bool expected)
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = ParcelStatus.Registered;

        // Act
        var result = parcel.CanTransitionTo(targetStatus);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CanTransitionFromOutForDelivery()
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = ParcelStatus.OutForDelivery;

        // Act & Assert
        parcel.CanTransitionTo(ParcelStatus.Delivered).Should().BeTrue();
        parcel.CanTransitionTo(ParcelStatus.FailedAttempt).Should().BeTrue();
        parcel.CanTransitionTo(ParcelStatus.Sorted).Should().BeFalse();
    }

    [Fact]
    public void CanTransitionFromDelivered_ShouldNotAllowAnyTransitions()
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = ParcelStatus.Delivered;

        // Act & Assert - Delivered is terminal state
        parcel.CanTransitionTo(ParcelStatus.Exception).Should().BeFalse();
        parcel.CanTransitionTo(ParcelStatus.ReturnedToDepot).Should().BeFalse();
    }
}