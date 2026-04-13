using FluentAssertions;
using LastMile.TMS.Application.Features.Parcels.Commands.ScanParcel;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Tests.Features.Parcels.Commands.ScanParcel;

public class ScanParcelCommandValidatorTests
{
    private readonly ScanParcelCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        // Arrange
        var command = new ScanParcelCommand(
            "LM-260411-ABC123", ParcelStatus.ReceivedAtDepot);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyTrackingNumber_Fails()
    {
        // Arrange
        var command = new ScanParcelCommand(
            "", ParcelStatus.ReceivedAtDepot);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("LM-12345-AB")]       // too short
    [InlineData("LM-260411-1234567")] // too long
    [InlineData("lm-260411-ABC123")]  // lowercase
    public void InvalidTrackingNumberFormat_Fails(string trackingNumber)
    {
        // Arrange
        var command = new ScanParcelCommand(
            trackingNumber, ParcelStatus.ReceivedAtDepot);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(ParcelStatus.Cancelled)]
    [InlineData(ParcelStatus.OutForDelivery)]
    [InlineData(ParcelStatus.Delivered)]
    [InlineData(ParcelStatus.FailedAttempt)]
    [InlineData(ParcelStatus.Exception)]
    public void NonDepotScanStatus_Fails(ParcelStatus status)
    {
        // Arrange
        var command = new ScanParcelCommand(
            "LM-260411-ABC123", status);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(ParcelStatus.ReceivedAtDepot)]
    [InlineData(ParcelStatus.Sorted)]
    [InlineData(ParcelStatus.Staged)]
    [InlineData(ParcelStatus.Loaded)]
    [InlineData(ParcelStatus.ReturnedToDepot)]
    public void ValidDepotScanStatus_Passes(ParcelStatus status)
    {
        // Arrange
        var command = new ScanParcelCommand(
            "LM-260411-ABC123", status);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
