using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Manifests.Commands.ReceiveParcel;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Manifests.Commands.ReceiveParcel;

public class ReceiveParcelCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ReceiveParcelCommandHandler _sut;

    public ReceiveParcelCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("test-user");

        _context = new TestDbContext(options, _currentUserService);
        _sut = new ReceiveParcelCommandHandler(_context, _currentUserService);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_MatchingItem_TransitionsToReceived()
    {
        // Arrange
        var (manifest, parcel) = await SetupManifestAndParcel();
        _context.ChangeTracker.Clear();

        var command = new ReceiveParcelCommand(manifest.Id, parcel.TrackingNumber);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ParcelId.Should().Be(parcel.Id);
        result.NewStatus.Should().Be(ParcelStatus.ReceivedAtDepot);
        result.ManifestItemStatus.Should().Be(ManifestItemStatus.Received);
    }

    [Fact]
    public async Task Handle_NoMatchingItem_CreatesUnexpected()
    {
        // Arrange
        var (manifest, _) = await SetupManifestAndParcel();
        var unexpectedParcel = CreateParcel();
        _context.Parcels.Add(unexpectedParcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ReceiveParcelCommand(manifest.Id, unexpectedParcel.TrackingNumber);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.ReceivedAtDepot);
        result.ManifestItemStatus.Should().Be(ManifestItemStatus.Unexpected);

        var updatedManifest = await _context.Manifests.Include(m => m.Items)
            .FirstAsync(m => m.Id == manifest.Id);
        updatedManifest.Items.Should().Contain(i => i.Status == ManifestItemStatus.Unexpected);
    }

    [Fact]
    public async Task Handle_AutoStartsIfManifestIsOpen()
    {
        // Arrange
        var (manifest, parcel) = await SetupManifestAndParcel();
        manifest.Status.Should().Be(ManifestStatus.Open); // verify initial state
        _context.ChangeTracker.Clear();

        var command = new ReceiveParcelCommand(manifest.Id, parcel.TrackingNumber);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedManifest = await _context.Manifests.FindAsync(manifest.Id);
        updatedManifest!.Status.Should().Be(ManifestStatus.Receiving);
        updatedManifest.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ParcelNotRegistered_Throws()
    {
        // Arrange
        var (manifest, parcel) = await SetupManifestAndParcel();
        parcel.Status = ParcelStatus.ReceivedAtDepot;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ReceiveParcelCommand(manifest.Id, parcel.TrackingNumber);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Registered*");
    }

    [Fact]
    public async Task Handle_CreatesTrackingEvent()
    {
        // Arrange
        var (manifest, parcel) = await SetupManifestAndParcel();
        _context.ChangeTracker.Clear();

        var command = new ReceiveParcelCommand(manifest.Id, parcel.TrackingNumber);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.ArrivedAtFacility);
        trackingEvent.Operator.Should().Be("test-user");
    }

    private async Task<(Manifest manifest, Parcel parcel)> SetupManifestAndParcel()
    {
        var depot = new Depot
        {
            Name = "Test Depot", IsActive = true,
            Address = new Address
            {
                Street1 = "123 Main St", City = "Almaty", State = "Almaty",
                PostalCode = "050000", CountryCode = "KZ"
            }
        };
        _context.Depots.Add(depot);

        var parcel = CreateParcel();
        _context.Parcels.Add(parcel);

        var manifest = Domain.Entities.Manifest.Create("Test Manifest", depot.Id);
        var item = ManifestItem.Create(manifest.Id, parcel.TrackingNumber);
        manifest.Items.Add(item);
        _context.Manifests.Add(manifest);

        await _context.SaveChangesAsync();
        return (manifest, parcel);
    }

    private static Parcel CreateParcel()
    {
        var parcel = Parcel.Create("Test", ServiceType.Standard);
        parcel.Weight = 1.0m;
        parcel.WeightUnit = WeightUnit.Kg;
        parcel.Length = 10m;
        parcel.Width = 10m;
        parcel.Height = 10m;
        parcel.DimensionUnit = DimensionUnit.Cm;
        parcel.DeclaredValue = 100m;
        parcel.ShipperAddress = new Address
        {
            Street1 = "123 Shipper St", City = "Almaty", State = "Almaty",
            PostalCode = "050000", CountryCode = "KZ"
        };
        parcel.RecipientAddress = new Address
        {
            Street1 = "456 Recipient St", City = "Astana", State = "Astana",
            PostalCode = "010000", CountryCode = "KZ"
        };
        return parcel;
    }
}
