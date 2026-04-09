using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
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
    private readonly ChangeParcelStatusCommandHandler _sut;

    public ChangeParcelStatusCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("test-user-123");

        _context = new TestDbContext(options, _currentUserService);
        _sut = new ChangeParcelStatusCommandHandler(_context, _currentUserService);
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
