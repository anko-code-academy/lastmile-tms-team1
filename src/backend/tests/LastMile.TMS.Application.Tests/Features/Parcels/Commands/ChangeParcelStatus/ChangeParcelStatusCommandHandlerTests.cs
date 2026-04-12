using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Application.Features.Parcels.Commands.ChangeParcelStatus;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Parcels.Commands.ChangeParcelStatus;

public class ChangeParcelStatusCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBinAssignmentService _binAssignmentService;
    private readonly ChangeParcelStatusCommandHandler _sut;

    public ChangeParcelStatusCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserName.Returns("test-user-123");

        _context = new TestDbContext(options, _currentUserService);
        _binAssignmentService = new BinAssignmentService(_context);
        _sut = new ChangeParcelStatusCommandHandler(_context, _currentUserService, _binAssignmentService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidTransition_CreatesTrackingEventAndUpdatesStatus()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.ReceivedAtDepot,
            "Almaty", "Almaty", "KZ", "Received at depot");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.ReceivedAtDepot);
        result.Id.Should().Be(parcel.Id);

        var trackingEvents = await _context.TrackingEvents
            .Where(te => te.ParcelId == parcel.Id)
            .ToListAsync();
        trackingEvents.Should().HaveCount(1);
        trackingEvents[0].EventType.Should().Be(EventType.ArrivedAtFacility);
        trackingEvents[0].Operator.Should().Be("test-user-123");
        trackingEvents[0].LocationCity.Should().Be("Almaty");
        trackingEvents[0].Description.Should().Be("Received at depot");
    }

    [Fact]
    public async Task Handle_TransitionToDelivered_SetsActualDeliveryDate()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.OutForDelivery);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.Delivered,
            Description: "Delivered to recipient");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.Delivered);

        var updatedParcel = await _context.Parcels.FindAsync(parcel.Id);
        updatedParcel!.ActualDeliveryDate.Should().NotBeNull();

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.Delivered);
    }

    [Fact]
    public async Task Handle_TransitionToFailedAttempt_IncrementsDeliveryAttempts()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.OutForDelivery);
        parcel.DeliveryAttempts = 0;
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.FailedAttempt,
            Description: "Recipient not available");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.DeliveryAttempts.Should().Be(1);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.DeliveryAttempted);
    }

    [Fact]
    public async Task Handle_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange - Registered cannot go directly to Delivered
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.Delivered);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        var trackingEvents = await _context.TrackingEvents
            .Where(te => te.ParcelId == parcel.Id)
            .ToListAsync();
        trackingEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ParcelNotFound_Throws()
    {
        // Arrange
        var command = new ChangeParcelStatusCommand(
            Guid.NewGuid(), ParcelStatus.ReceivedAtDepot);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_TransitionToCancelled_Throws()
    {
        // Arrange - Cancelled should use CancelParcelCommand
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.Cancelled,
            Description: "Trying to cancel via wrong command");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CancelParcel*");
    }

    [Fact]
    public async Task Handle_TransitionToSorted_AssignsParcelToBin()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var aisle = new Aisle { Name = "Aisle A1", ZoneId = zone.Id, Order = 1 };
        aisle.SetLabel("A", "A");
        var bin = new Bin { Slot = 1, Capacity = 10, IsActive = true, ZoneId = zone.Id, AisleId = aisle.Id };
        bin.SetLabel(aisle.Label);
        var parcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        parcel.ZoneId = zone.Id;

        _context.Zones.Add(zone);
        _context.Aisles.Add(aisle);
        _context.Bins.Add(bin);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.Sorted,
            Description: "Sorted by zone");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.Sorted);

        var updatedParcel = await _context.Parcels.FindAsync(parcel.Id);
        updatedParcel!.BinId.Should().Be(bin.Id);
    }

    [Fact]
    public async Task Handle_TransitionFromSorted_RemovesFromBin()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var aisle = new Aisle { Name = "Aisle A1", ZoneId = zone.Id, Order = 1 };
        aisle.SetLabel("A", "A");
        var bin = new Bin { Slot = 1, Capacity = 10, IsActive = true, ZoneId = zone.Id, AisleId = aisle.Id };
        bin.SetLabel(aisle.Label);
        var parcel = CreateParcel(ParcelStatus.Sorted);
        parcel.ZoneId = zone.Id;
        parcel.BinId = bin.Id;

        _context.Zones.Add(zone);
        _context.Aisles.Add(aisle);
        _context.Bins.Add(bin);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.Staged,
            Description: "Staged for loading");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.Staged);

        var updatedParcel = await _context.Parcels.FindAsync(parcel.Id);
        updatedParcel!.BinId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_TransitionToSorted_NoBinAvailable_TransitionsToException()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var parcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        parcel.ZoneId = zone.Id;

        _context.Zones.Add(zone);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new ChangeParcelStatusCommand(
            parcel.Id, ParcelStatus.Sorted,
            Description: "Sorted by zone");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.Exception);

        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.EventType.Should().Be(EventType.Exception);
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
