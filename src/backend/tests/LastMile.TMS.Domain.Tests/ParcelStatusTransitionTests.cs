using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class ParcelStatusTransitionTests
{
    [Theory]
    [InlineData(ParcelStatus.Registered)]
    [InlineData(ParcelStatus.ReceivedAtDepot)]
    [InlineData(ParcelStatus.Sorted)]
    [InlineData(ParcelStatus.Staged)]
    public void CanCancel_ShouldAllow_WhenInEarlyStatuses(ParcelStatus initialStatus)
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = initialStatus;

        // Act
        var canCancel = parcel.CanTransitionTo(ParcelStatus.Cancelled);

        // Assert
        canCancel.Should().BeTrue($"Cancellation should be allowed from {initialStatus} status");
    }

    [Theory]
    [InlineData(ParcelStatus.Loaded)]
    [InlineData(ParcelStatus.OutForDelivery)]
    [InlineData(ParcelStatus.Delivered)]
    [InlineData(ParcelStatus.FailedAttempt)]
    [InlineData(ParcelStatus.Exception)]
    [InlineData(ParcelStatus.ReturnedToDepot)]
    public void CanCancel_ShouldBlock_WhenInLateStatuses(ParcelStatus initialStatus)
    {
        // Arrange
        var parcel = Parcel.Create(null, ServiceType.Standard);
        parcel.Status = initialStatus;

        // Act
        var canCancel = parcel.CanTransitionTo(ParcelStatus.Cancelled);

        // Assert
        canCancel.Should().BeFalse($"Cancellation should be blocked from {initialStatus} status");
    }
}
