using FluentAssertions;
using LastMile.TMS.Application.Common.DTOs;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Parcels.Commands.UpdateParcel;

public class UpdateParcelCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UpdateParcelCommandHandler _sut;

    public UpdateParcelCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserName.Returns("admin@lastmile.tms");

        _context = new TestDbContext(options, _currentUserService);
        _sut = new UpdateParcelCommandHandler(_context, _currentUserService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_SingleScalarChange_CreatesOneAuditLog()
    {
        // Arrange
        var parcel = CreateParcel();
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateParcelCommand(
            parcel.Id, Description: null, Weight: 5.0m,
            Length: null, Width: null, Height: null,
            ServiceType: null, ParcelType: null,
            ShipperAddress: null, RecipientAddress: null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var auditLogs = await _context.ParcelAuditLogs
            .Where(al => al.ParcelId == parcel.Id)
            .ToListAsync();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].PropertyName.Should().Be("Weight");
        auditLogs[0].OldValue.Should().Be("1.0");
        auditLogs[0].NewValue.Should().Be("5.0");
        auditLogs[0].ChangedBy.Should().Be("admin@lastmile.tms");
    }

    [Fact]
    public async Task Handle_MultipleScalarChanges_CreatesMultipleAuditLogs()
    {
        // Arrange
        var parcel = CreateParcel();
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateParcelCommand(
            parcel.Id, Description: "New description", Weight: 5.0m,
            Length: null, Width: null, Height: null,
            ServiceType: null, ParcelType: null,
            ShipperAddress: null, RecipientAddress: null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var auditLogs = await _context.ParcelAuditLogs
            .Where(al => al.ParcelId == parcel.Id)
            .ToListAsync();
        auditLogs.Should().HaveCount(2);
        auditLogs.Select(al => al.PropertyName).Should().Contain(["Description", "Weight"]);
    }

    [Fact]
    public async Task Handle_AddressFieldChange_CreatesPerFieldAuditLog()
    {
        // Arrange
        var parcel = CreateParcel();
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var newAddress = new AddressInput(
            Street1: "999 New St",
            Street2: null,
            City: "Almaty",
            State: "Almaty",
            PostalCode: "050000",
            CountryCode: "KZ",
            IsResidential: false,
            ContactName: null,
            CompanyName: null,
            Phone: null,
            Email: null);

        var command = new UpdateParcelCommand(
            parcel.Id, Description: null, Weight: null,
            Length: null, Width: null, Height: null,
            ServiceType: null, ParcelType: null,
            ShipperAddress: newAddress, RecipientAddress: null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var auditLogs = await _context.ParcelAuditLogs
            .Where(al => al.ParcelId == parcel.Id)
            .ToListAsync();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].PropertyName.Should().Be("ShipperAddress.Street1");
        auditLogs[0].OldValue.Should().Be("123 Shipper St");
        auditLogs[0].NewValue.Should().Be("999 New St");
        auditLogs[0].ChangedBy.Should().Be("admin@lastmile.tms");
    }

    [Fact]
    public async Task Handle_MultipleAddressFieldChanges_CreatesPerFieldAuditLogs()
    {
        // Arrange
        var parcel = CreateParcel();
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var newAddress = new AddressInput(
            Street1: "999 New St",
            Street2: null,
            City: "Shymkent",
            State: "Almaty",
            PostalCode: "050000",
            CountryCode: "KZ",
            IsResidential: false,
            ContactName: null,
            CompanyName: null,
            Phone: null,
            Email: null);

        var command = new UpdateParcelCommand(
            parcel.Id, Description: null, Weight: null,
            Length: null, Width: null, Height: null,
            ServiceType: null, ParcelType: null,
            ShipperAddress: newAddress, RecipientAddress: null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var auditLogs = await _context.ParcelAuditLogs
            .Where(al => al.ParcelId == parcel.Id)
            .ToListAsync();
        auditLogs.Should().HaveCount(2);
        auditLogs.Select(al => al.PropertyName).Should().Contain(["ShipperAddress.Street1", "ShipperAddress.City"]);
    }

    [Fact]
    public async Task Handle_NoChanges_NoAuditLogs()
    {
        // Arrange
        var parcel = CreateParcel();
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateParcelCommand(
            parcel.Id, Description: "Test parcel", Weight: 1.0m,
            Length: 10m, Width: 10m, Height: 10m,
            ServiceType: ServiceType.Standard, ParcelType: null,
            ShipperAddress: null, RecipientAddress: null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var auditLogs = await _context.ParcelAuditLogs
            .Where(al => al.ParcelId == parcel.Id)
            .ToListAsync();
        auditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ParcelNotFound_Throws()
    {
        // Arrange
        var command = new UpdateParcelCommand(
            Guid.NewGuid(), Description: null, Weight: null,
            Length: null, Width: null, Height: null,
            ServiceType: null, ParcelType: null,
            ShipperAddress: null, RecipientAddress: null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WrongStatus_Throws()
    {
        // Arrange
        var parcel = CreateParcel();
        parcel.Status = ParcelStatus.Delivered;
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateParcelCommand(
            parcel.Id, Description: "Updated", Weight: null,
            Length: null, Width: null, Height: null,
            ServiceType: null, ParcelType: null,
            ShipperAddress: null, RecipientAddress: null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot edit parcel*");
    }

    private static Parcel CreateParcel()
    {
        var parcel = Parcel.Create("Test parcel", ServiceType.Standard);
        parcel.Weight = 1.0m;
        parcel.Length = 10m;
        parcel.Width = 10m;
        parcel.Height = 10m;
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
