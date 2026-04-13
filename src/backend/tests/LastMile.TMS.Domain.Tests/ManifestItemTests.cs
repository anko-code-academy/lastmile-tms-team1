using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class ManifestItemTests
{
    [Fact]
    public void Create_ShouldSetStatusExpected()
    {
        // Arrange
        var manifestId = Guid.CreateVersion7();

        // Act
        var item = ManifestItem.Create(manifestId, "LM-260411-ABC123");

        // Assert
        item.ManifestId.Should().Be(manifestId);
        item.TrackingNumber.Should().Be("LM-260411-ABC123");
        item.Status.Should().Be(ManifestItemStatus.Expected);
        item.ParcelId.Should().BeNull();
    }

    [Fact]
    public void MarkReceived_ShouldTransitionToReceivedAndSetParcelId()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");
        var parcelId = Guid.CreateVersion7();

        // Act
        item.MarkReceived(parcelId);

        // Assert
        item.Status.Should().Be(ManifestItemStatus.Received);
        item.ParcelId.Should().Be(parcelId);
    }

    [Fact]
    public void MarkUnexpected_ShouldTransitionToUnexpectedAndSetParcelId()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");
        var parcelId = Guid.CreateVersion7();

        // Act
        item.MarkUnexpected(parcelId);

        // Assert
        item.Status.Should().Be(ManifestItemStatus.Unexpected);
        item.ParcelId.Should().Be(parcelId);
    }

    [Fact]
    public void MarkMissing_ShouldTransitionToMissing()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");

        // Act
        item.MarkMissing();

        // Assert
        item.Status.Should().Be(ManifestItemStatus.Missing);
        item.ParcelId.Should().BeNull();
    }

    [Fact]
    public void MarkReceived_WhenAlreadyReceived_ShouldThrow()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");
        item.MarkReceived(Guid.CreateVersion7());

        // Act
        var act = () => item.MarkReceived(Guid.CreateVersion7());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already*");
    }

    [Fact]
    public void MarkUnexpected_WhenAlreadyReceived_ShouldThrow()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");
        item.MarkReceived(Guid.CreateVersion7());

        // Act
        var act = () => item.MarkUnexpected(Guid.CreateVersion7());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkMissing_WhenAlreadyReceived_ShouldThrow()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");
        item.MarkReceived(Guid.CreateVersion7());

        // Act
        var act = () => item.MarkMissing();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkMissing_WhenAlreadyMissing_ShouldThrow()
    {
        // Arrange
        var item = ManifestItem.Create(Guid.CreateVersion7(), "LM-260411-ABC123");
        item.MarkMissing();

        // Act
        var act = () => item.MarkMissing();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
