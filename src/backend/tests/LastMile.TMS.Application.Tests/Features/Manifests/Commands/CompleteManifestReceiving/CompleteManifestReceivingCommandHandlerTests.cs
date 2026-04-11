using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Manifests.Commands.CompleteManifestReceiving;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Manifests.Commands.CompleteManifestReceiving;

public class CompleteManifestReceivingCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly CompleteManifestReceivingCommandHandler _sut;

    public CompleteManifestReceivingCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("test-user");

        _context = new TestDbContext(options, _currentUserService);
        _sut = new CompleteManifestReceivingCommandHandler(_context, _currentUserService);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_RemainingExpectedItems_MarkedMissing()
    {
        // Arrange
        var (manifest, _, _) = await SetupManifestWithReceivedAndExpected();
        _context.ChangeTracker.Clear();

        var command = new CompleteManifestReceivingCommand(manifest.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ManifestStatus.Completed);
        result.MissingCount.Should().Be(1);
        result.ReceivedCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MissingParcelParcels_TransitionToException()
    {
        // Arrange
        var (manifest, _, missingParcel) = await SetupManifestWithReceivedAndExpected();
        _context.ChangeTracker.Clear();

        var command = new CompleteManifestReceivingCommand(manifest.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedParcel = await _context.Parcels.FindAsync(missingParcel.Id);
        updatedParcel!.Status.Should().Be(ParcelStatus.Exception);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == missingParcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.Exception);
    }

    [Fact]
    public async Task Handle_ManifestNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new CompleteManifestReceivingCommand(Guid.NewGuid());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private async Task<(Manifest manifest, Parcel receivedParcel, Parcel missingParcel)>
        SetupManifestWithReceivedAndExpected()
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

        var receivedParcel = CreateParcel();
        var missingParcel = CreateParcel();
        _context.Parcels.AddRange(receivedParcel, missingParcel);

        var manifest = Domain.Entities.Manifest.Create("Test Manifest", depot.Id);
        manifest.StartReceiving();

        var receivedItem = ManifestItem.Create(manifest.Id, receivedParcel.TrackingNumber);
        receivedItem.MarkReceived(receivedParcel.Id);
        var missingItem = ManifestItem.Create(manifest.Id, missingParcel.TrackingNumber);

        manifest.Items.Add(receivedItem);
        manifest.Items.Add(missingItem);
        _context.Manifests.Add(manifest);

        await _context.SaveChangesAsync();
        return (manifest, receivedParcel, missingParcel);
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
