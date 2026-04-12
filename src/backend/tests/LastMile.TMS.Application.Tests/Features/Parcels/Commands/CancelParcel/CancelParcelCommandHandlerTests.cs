using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Parcels.Commands.CancelParcel;

public class CancelParcelCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBinAssignmentService _binAssignmentService;
    private readonly CancelParcelCommandHandler _sut;

    public CancelParcelCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserName.Returns("admin@lastmile.tms");

        _context = new TestDbContext(options, _currentUserService);
        _binAssignmentService = new BinAssignmentService(_context);
        _sut = new CancelParcelCommandHandler(_context, _currentUserService, _binAssignmentService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidCancel_SetsStatusToCancelled()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CancelParcelCommand(parcel.Id, "Wrong address");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.Cancelled);
        result.Id.Should().Be(parcel.Id);
    }

    [Fact]
    public async Task Handle_ValidCancel_CreatesAuditLogWithUserName()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CancelParcelCommand(parcel.Id, "Wrong address");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var auditLog = await _context.ParcelAuditLogs
            .FirstOrDefaultAsync(al => al.ParcelId == parcel.Id);
        auditLog.Should().NotBeNull();
        auditLog!.PropertyName.Should().Be("Status");
        auditLog.ChangedBy.Should().Be("admin@lastmile.tms");
    }

    [Fact]
    public async Task Handle_ValidCancel_CreatesTrackingEventWithUserName()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.Registered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CancelParcelCommand(parcel.Id, "Wrong address");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var trackingEvent = await _context.TrackingEvents
            .FirstOrDefaultAsync(te => te.ParcelId == parcel.Id);
        trackingEvent.Should().NotBeNull();
        trackingEvent!.Operator.Should().Be("admin@lastmile.tms");
        trackingEvent.EventType.Should().Be(EventType.Exception);
    }

    [Fact]
    public async Task Handle_InvalidStatus_Throws()
    {
        // Arrange - Delivered parcel cannot be cancelled
        var parcel = CreateParcel(ParcelStatus.Delivered);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CancelParcelCommand(parcel.Id, "Trying to cancel");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot cancel*");
    }

    [Fact]
    public async Task Handle_ParcelNotFound_Throws()
    {
        // Arrange
        var command = new CancelParcelCommand(Guid.NewGuid(), "Not found");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_CancelFromSorted_RemovesFromBin()
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

        var command = new CancelParcelCommand(parcel.Id, "Customer requested cancellation");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ParcelStatus.Cancelled);

        var updatedParcel = await _context.Parcels.FindAsync(parcel.Id);
        updatedParcel!.BinId.Should().BeNull();
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
