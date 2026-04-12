using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Parcels.Commands.ScanParcel;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Parcels.Commands.ScanParcel;

public class ScanParcelCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ScanParcelCommandHandler _sut;

    public ScanParcelCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("test-user-123");

        _context = new TestDbContext(options, _currentUserService);
        _sut = new ScanParcelCommandHandler(_context, _currentUserService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidTrackingNumber_TransitionsToReceivedAtDepot()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.ReceivedAtDepot);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(parcel.Id);
        result.TrackingNumber.Should().Be(parcel.TrackingNumber);
        result.PreviousStatus.Should().Be(ParcelStatus.Registered);
        result.NewStatus.Should().Be(ParcelStatus.ReceivedAtDepot);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.ArrivedAtFacility);
        trackingEvent.Operator.Should().Be("test-user-123");
    }

    [Fact]
    public async Task Handle_ValidTrackingNumber_TransitionsSorted()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var bin = new Bin { Slot = 1, Capacity = 100, IsActive = true, ZoneId = zone.Id, AisleId = Guid.CreateVersion7() };
        bin.SetLabel("D1-A-A1");
        var parcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        parcel.ZoneId = zone.Id;
        parcel.Zone = zone;
        parcel.BinId = bin.Id;
        parcel.Bin = bin;

        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.Sorted);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.Sorted);
        result.ZoneName.Should().Be("Zone A");
        result.BinLabel.Should().Be("D1-A-A1-01");
    }

    [Fact]
    public async Task Handle_UnknownTrackingNumber_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new ScanParcelCommand(
            "LM-999999-NOTFND", ParcelStatus.ReceivedAtDepot);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_InvalidTransition_TransitionsToException()
    {
        // Arrange - Registered cannot go directly to Loaded
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.Loaded);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.Exception);
        result.PreviousStatus.Should().Be(ParcelStatus.Registered);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.Exception);
    }

    [Fact]
    public async Task Handle_TransitionToCancelled_Throws()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.Cancelled);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CancelParcel*");
    }

    [Fact]
    public async Task Handle_WithRouteId_ValidatesParcelBelongsToRoute()
    {
        // Arrange
        var route = new Route
        {
            Name = "Route 1", Status = RouteStatus.Draft,
            PlannedStartTime = DateTime.UtcNow.AddDays(1), ZoneId = Guid.CreateVersion7()
        };
        var stop = new RouteStop
        {
            SequenceNumber = 1, Status = RouteStopStatus.Pending,
            Street1 = "123 Main St", RouteId = route.Id, Route = route
        };
        var parcel = CreateParcel(ParcelStatus.Sorted);
        parcel.RouteStopId = stop.Id;
        parcel.RouteStop = stop;

        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.Staged, RouteId: route.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.Staged);
    }

    [Fact]
    public async Task Handle_WithRouteId_WrongRoute_TransitionsToException()
    {
        // Arrange
        var route1 = new Route
        {
            Name = "Route 1", Status = RouteStatus.Draft,
            PlannedStartTime = DateTime.UtcNow.AddDays(1), ZoneId = Guid.CreateVersion7()
        };
        var route2 = new Route
        {
            Name = "Route 2", Status = RouteStatus.Draft,
            PlannedStartTime = DateTime.UtcNow.AddDays(1), ZoneId = Guid.CreateVersion7()
        };
        var stop = new RouteStop
        {
            SequenceNumber = 1, Status = RouteStopStatus.Pending,
            Street1 = "123 Main St", RouteId = route1.Id, Route = route1
        };
        var parcel = CreateParcel(ParcelStatus.Sorted);
        parcel.RouteStopId = stop.Id;
        parcel.RouteStop = stop;

        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.Staged, RouteId: route2.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.Exception);
        result.PreviousStatus.Should().Be(ParcelStatus.Sorted);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.Exception);
    }

    [Fact]
    public async Task Handle_ParcelNotOnManifest_TransitionsToException()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        var manifest = Manifest.Create("Test Manifest", Guid.CreateVersion7());
        // Manifest has a different tracking number, not our parcel
        manifest.Items.Add(ManifestItem.Create(manifest.Id, "LM-260411-OTHER1"));

        _context.Parcels.Add(parcel);
        _context.Manifests.Add(manifest);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.ReceivedAtDepot,
            ManifestId: manifest.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.Exception);
        result.PreviousStatus.Should().Be(ParcelStatus.Registered);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.Exception);
    }

    [Fact]
    public async Task Handle_ParcelOnManifest_TransitionsToReceivedAtDepot()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        var manifest = Manifest.Create("Test Manifest", Guid.CreateVersion7());
        manifest.Items.Add(ManifestItem.Create(manifest.Id, parcel.TrackingNumber));

        _context.Parcels.Add(parcel);
        _context.Manifests.Add(manifest);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.ReceivedAtDepot,
            ManifestId: manifest.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.NewStatus.Should().Be(ParcelStatus.ReceivedAtDepot);
        result.PreviousStatus.Should().Be(ParcelStatus.Registered);
    }

    [Fact]
    public async Task Handle_ReturnsRouteName()
    {
        // Arrange
        var route = new Route
        {
            Name = "Route Alpha", Status = RouteStatus.Draft,
            PlannedStartTime = DateTime.UtcNow.AddDays(1), ZoneId = Guid.CreateVersion7()
        };
        var stop = new RouteStop
        {
            SequenceNumber = 1, Status = RouteStopStatus.Pending,
            Street1 = "123 Main St", RouteId = route.Id, Route = route
        };
        var parcel = CreateParcel(ParcelStatus.Sorted);
        parcel.RouteStopId = stop.Id;
        parcel.RouteStop = stop;

        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ScanParcelCommand(
            parcel.TrackingNumber, ParcelStatus.Staged);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.RouteName.Should().Be("Route Alpha");
    }

    private static Parcel CreateParcel(ParcelStatus status)
    {
        var parcel = Parcel.Create("Test parcel", ServiceType.Standard);
        parcel.Status = status;
        parcel.Weight = 1.0m;
        parcel.WeightUnit = WeightUnit.Kg;
        parcel.Length = 10m;
        parcel.Width = 10m;
        parcel.Height = 10m;
        parcel.DimensionUnit = DimensionUnit.Cm;
        parcel.DeclaredValue = 100m;
        parcel.ShipperAddress = new Address
        {
            Street1 = "123 Shipper St",
            City = "Almaty",
            State = "Almaty",
            PostalCode = "050000",
            CountryCode = "KZ"
        };
        parcel.RecipientAddress = new Address
        {
            Street1 = "456 Recipient St",
            City = "Astana",
            State = "Astana",
            PostalCode = "010000",
            CountryCode = "KZ"
        };
        return parcel;
    }
}
