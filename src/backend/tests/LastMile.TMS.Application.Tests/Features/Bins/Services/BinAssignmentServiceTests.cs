using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Bins.Services;

public class BinAssignmentServiceTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly BinAssignmentService _sut;

    public BinAssignmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns("test-user-123");

        _context = new TestDbContext(options, currentUserService);
        _sut = new BinAssignmentService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AssignToBinAsync_WithAvailableBin_AssignsParcelToBin()
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

        // Act
        var result = await _sut.AssignToBinAsync(parcel, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        parcel.BinId.Should().Be(bin.Id);
    }

    [Fact]
    public async Task AssignToBinAsync_NoBinForZone_ReturnsFalse()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var parcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        parcel.ZoneId = zone.Id;

        _context.Zones.Add(zone);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _sut.AssignToBinAsync(parcel, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        parcel.BinId.Should().BeNull();
    }

    [Fact]
    public async Task AssignToBinAsync_AllBinsFull_ReturnsFalse()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var aisle = new Aisle { Name = "Aisle A1", ZoneId = zone.Id, Order = 1 };
        aisle.SetLabel("A", "A");
        var bin = new Bin { Slot = 1, Capacity = 1, IsActive = true, ZoneId = zone.Id, AisleId = aisle.Id };
        bin.SetLabel(aisle.Label);

        // Fill the bin to capacity
        var existingParcel = CreateParcel(ParcelStatus.Sorted);
        existingParcel.ZoneId = zone.Id;
        existingParcel.BinId = bin.Id;

        var newParcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        newParcel.ZoneId = zone.Id;

        _context.Zones.Add(zone);
        _context.Aisles.Add(aisle);
        _context.Bins.Add(bin);
        _context.Parcels.Add(existingParcel);
        _context.Parcels.Add(newParcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _sut.AssignToBinAsync(newParcel, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        newParcel.BinId.Should().BeNull();
    }

    [Fact]
    public async Task AssignToBinAsync_ParcelHasNoZone_ReturnsFalse()
    {
        // Arrange
        var parcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        parcel.ZoneId = null;

        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _sut.AssignToBinAsync(parcel, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        parcel.BinId.Should().BeNull();
    }

    [Fact]
    public async Task AssignToBinAsync_InactiveBinNotConsidered_ReturnsFalse()
    {
        // Arrange
        var zone = new Zone { Name = "Zone A", IsActive = true, DepotId = Guid.CreateVersion7() };
        var aisle = new Aisle { Name = "Aisle A1", ZoneId = zone.Id, Order = 1 };
        aisle.SetLabel("A", "A");
        var bin = new Bin { Slot = 1, Capacity = 10, IsActive = false, ZoneId = zone.Id, AisleId = aisle.Id };
        bin.SetLabel(aisle.Label);
        var parcel = CreateParcel(ParcelStatus.ReceivedAtDepot);
        parcel.ZoneId = zone.Id;

        _context.Zones.Add(zone);
        _context.Aisles.Add(aisle);
        _context.Bins.Add(bin);
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _sut.AssignToBinAsync(parcel, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        parcel.BinId.Should().BeNull();
    }

    [Fact]
    public void RemoveFromBin_ClearsBinId()
    {
        // Arrange
        var binId = Guid.CreateVersion7();
        var parcel = CreateParcel(ParcelStatus.Sorted);
        parcel.BinId = binId;

        // Act
        _sut.RemoveFromBin(parcel);

        // Assert
        parcel.BinId.Should().BeNull();
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
