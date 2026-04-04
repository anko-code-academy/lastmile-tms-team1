using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests;

public class TestDbContext : AppDbContext
{
    public TestDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options, currentUserService) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore Npgsql-specific shadow properties not supported by in-memory provider
        modelBuilder.Entity<Address>().Ignore("RecipientNameSearchVector");
        modelBuilder.Entity<Address>().Ignore("AddressSearchVector");
    }
}

public class SoftDeleteBehaviorTests : IDisposable
{
    private readonly TestDbContext _context;

    public SoftDeleteBehaviorTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns("test-user-123");

        _context = new TestDbContext(options, currentUserService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_NewEntity_ShouldSetCreatedAtAndCreatedBy()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };

        // Act
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        // Assert
        address.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        address.CreatedBy.Should().Be("test-user-123");
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_ShouldSetLastModifiedAtAndLastModifiedBy()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        // Clear change tracker to simulate a new context instance
        _context.ChangeTracker.Clear();

        // Act - re-attach and mark as modified
        address.City = "Shelbyville";
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();

        // Assert
        address.LastModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        address.LastModifiedBy.Should().Be("test-user-123");
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedEntity_ShouldSoftDelete()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var addressId = address.Id;

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act
        var entityToDelete = await _context.Addresses.FindAsync(addressId);
        _context.Addresses.Remove(entityToDelete!);
        await _context.SaveChangesAsync();

        // Assert
        entityToDelete.IsDeleted.Should().BeTrue();
        entityToDelete.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entityToDelete.DeletedBy.Should().Be("test-user-123");
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedEntity_ShouldNotRemoveFromDatabase()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var addressId = address.Id;

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act
        var entityToDelete = await _context.Addresses.FindAsync(addressId);
        _context.Addresses.Remove(entityToDelete!);
        await _context.SaveChangesAsync();

        // Assert - using IgnoreQueryFilters to see the soft-deleted entity
        var allAddresses = _context.Addresses.IgnoreQueryFilters().ToList();
        allAddresses.Should().Contain(a => a.Id == addressId);
    }

    [Fact]
    public async Task QueryFilter_ShouldExcludeSoftDeletedEntities()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var addressId = address.Id;

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Soft delete the entity
        var entityToDelete = await _context.Addresses.FindAsync(addressId);
        _context.Addresses.Remove(entityToDelete!);
        await _context.SaveChangesAsync();

        // Clear change tracker again
        _context.ChangeTracker.Clear();

        // Act
        var addresses = _context.Addresses.ToList();

        // Assert
        addresses.Should().NotContain(a => a.Id == addressId);
    }

    [Fact]
    public async Task IgnoreQueryFilters_ShouldReturnSoftDeletedEntities()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var addressId = address.Id;

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Soft delete the entity
        var entityToDelete = await _context.Addresses.FindAsync(addressId);
        _context.Addresses.Remove(entityToDelete!);
        await _context.SaveChangesAsync();

        // Clear change tracker again
        _context.ChangeTracker.Clear();

        // Act
        var allAddresses = _context.Addresses.IgnoreQueryFilters().ToList();

        // Assert
        allAddresses.Should().Contain(a => a.Id == addressId);
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedDepot_ShouldSoftDeleteWithAllProperties()
    {
        // Arrange
        var depot = new Depot
        {
            Name = "Main Depot",
            IsActive = true,
            ShiftSchedules = []
        };
        _context.Depots.Add(depot);
        await _context.SaveChangesAsync();

        var depotId = depot.Id;

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act
        var entityToDelete = await _context.Depots.FindAsync(depotId);
        _context.Depots.Remove(entityToDelete!);
        await _context.SaveChangesAsync();

        // Assert
        entityToDelete.IsDeleted.Should().BeTrue();
        entityToDelete.DeletedAt.Should().NotBeNull();
        entityToDelete.DeletedBy.Should().Be("test-user-123");
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedDriver_ShouldSoftDelete()
    {
        // Arrange
        var driver = new Driver
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@driver.com",
            Phone = "+1234567890",
            LicenseNumber = "DL123",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = Guid.NewGuid(),
            DepotId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsActive = true
        };
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        var driverId = driver.Id;

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act
        var entityToDelete = await _context.Drivers.FindAsync(driverId);
        _context.Drivers.Remove(entityToDelete!);
        await _context.SaveChangesAsync();

        // Assert
        entityToDelete.IsDeleted.Should().BeTrue();
        entityToDelete.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entityToDelete.DeletedBy.Should().Be("test-user-123");
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEntities_ShouldSetCorrectAuditFields()
    {
        // Arrange
        var address1 = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };
        var address2 = new Address
        {
            Street1 = "456 Oak Ave",
            City = "Shelbyville",
            State = "IL",
            PostalCode = "62702",
            CountryCode = "US"
        };

        _context.Addresses.AddRange(address1, address2);
        await _context.SaveChangesAsync();

        // Assert both have audit fields
        address1.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        address2.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        address1.CreatedBy.Should().Be("test-user-123");
        address2.CreatedBy.Should().Be("test-user-123");
    }
}
