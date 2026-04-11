using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class ManifestTests
{
    [Fact]
    public void Create_ShouldSetStatusOpen()
    {
        // Arrange
        var depotId = Guid.CreateVersion7();

        // Act
        var manifest = Manifest.Create("Test Manifest", depotId);

        // Assert
        manifest.Name.Should().Be("Test Manifest");
        manifest.DepotId.Should().Be(depotId);
        manifest.Status.Should().Be(ManifestStatus.Open);
        manifest.StartedAt.Should().BeNull();
        manifest.CompletedAt.Should().BeNull();
        manifest.Items.Should().BeEmpty();
    }

    [Fact]
    public void StartReceiving_ShouldTransitionToReceivingAndSetStartedAt()
    {
        // Arrange
        var manifest = Manifest.Create("Test", Guid.CreateVersion7());

        // Act
        manifest.StartReceiving();

        // Assert
        manifest.Status.Should().Be(ManifestStatus.Receiving);
        manifest.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void StartReceiving_FromCompleted_ShouldThrow()
    {
        // Arrange
        var manifest = Manifest.Create("Test", Guid.CreateVersion7());
        manifest.StartReceiving();
        manifest.Complete();

        // Act
        var act = () => manifest.StartReceiving();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start receiving*");
    }

    [Fact]
    public void StartReceiving_WhenAlreadyReceiving_ShouldThrow()
    {
        // Arrange
        var manifest = Manifest.Create("Test", Guid.CreateVersion7());
        manifest.StartReceiving();

        // Act
        var act = () => manifest.StartReceiving();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_ShouldTransitionToCompletedAndSetCompletedAt()
    {
        // Arrange
        var manifest = Manifest.Create("Test", Guid.CreateVersion7());
        manifest.StartReceiving();

        // Act
        manifest.Complete();

        // Assert
        manifest.Status.Should().Be(ManifestStatus.Completed);
        manifest.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_FromOpen_ShouldThrow()
    {
        // Arrange
        var manifest = Manifest.Create("Test", Guid.CreateVersion7());

        // Act
        var act = () => manifest.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot complete*");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        // Arrange
        var manifest = Manifest.Create("Test", Guid.CreateVersion7());
        manifest.StartReceiving();
        manifest.Complete();

        // Act
        var act = () => manifest.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
