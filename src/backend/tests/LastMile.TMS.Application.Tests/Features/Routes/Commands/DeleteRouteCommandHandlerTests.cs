using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Application.Features.Routes.Commands;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Routes.Commands;

public class DeleteRouteCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IBinAssignmentService _binAssignmentService;
    private readonly DeleteRouteCommandHandler _sut;

    public DeleteRouteCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns("test-user-123");

        _context = new TestDbContext(options, currentUserService);
        _binAssignmentService = new BinAssignmentService(_context);
        _sut = new DeleteRouteCommandHandler(_context, _binAssignmentService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_DeletedRoute_ParcelsRevertToSortedAndGetBin()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var aisle = new Aisle { Name = "Aisle A1", ZoneId = zone.Id, Order = 1 };
        aisle.SetLabel("A", "A");
        var bin = new Bin { Slot = 1, Capacity = 10, IsActive = true, ZoneId = zone.Id, AisleId = aisle.Id };
        bin.SetLabel(aisle.Label);

        var route = new Route
        {
            Name = "Route 1", Status = RouteStatus.Draft,
            PlannedStartTime = DateTime.UtcNow.AddDays(1), ZoneId = zone.Id
        };
        var stop = new RouteStop
        {
            SequenceNumber = 1, Status = RouteStopStatus.Pending,
            Street1 = "123 Main St", RouteId = route.Id, Route = route
        };
        var parcel = CreateParcel(ParcelStatus.Staged);
        parcel.ZoneId = zone.Id;
        parcel.RouteStopId = stop.Id;
        parcel.RouteStop = stop;

        _context.Zones.Add(zone);
        _context.Aisles.Add(aisle);
        _context.Bins.Add(bin);
        _context.Routes.Add(route);
        _context.RouteStops.Add(stop);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new DeleteRouteCommand(route.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var updatedParcel = await _context.Parcels.FindAsync(parcel.Id);
        updatedParcel!.Status.Should().Be(ParcelStatus.Sorted);
        updatedParcel.BinId.Should().Be(bin.Id);
    }

    [Fact]
    public async Task Handle_DeletedRoute_NoBinAvailable_TransitionsToException()
    {
        // Arrange - zone has no bins
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var route = new Route
        {
            Name = "Route 1", Status = RouteStatus.Draft,
            PlannedStartTime = DateTime.UtcNow.AddDays(1), ZoneId = zone.Id
        };
        var stop = new RouteStop
        {
            SequenceNumber = 1, Status = RouteStopStatus.Pending,
            Street1 = "123 Main St", RouteId = route.Id, Route = route
        };
        var parcel = CreateParcel(ParcelStatus.Staged);
        parcel.ZoneId = zone.Id;
        parcel.RouteStopId = stop.Id;
        parcel.RouteStop = stop;

        _context.Zones.Add(zone);
        _context.Routes.Add(route);
        _context.RouteStops.Add(stop);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new DeleteRouteCommand(route.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var updatedParcel = await _context.Parcels.FindAsync(parcel.Id);
        updatedParcel!.Status.Should().Be(ParcelStatus.Exception);
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
