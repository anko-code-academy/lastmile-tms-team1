using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class SoftDeleteTests
{
    [Fact]
    public void User_NewUser_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var user = User.Create("John", "Doe", "john@lastmile.com");

        // Assert
        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Role_NewRole_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var role = Role.Create("Admin");

        // Assert
        role.IsDeleted.Should().BeFalse();
        role.DeletedAt.Should().BeNull();
        role.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Depot_NewDepot_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var depot = new Depot { Name = "Main Depot" };

        // Assert
        depot.IsDeleted.Should().BeFalse();
        depot.DeletedAt.Should().BeNull();
        depot.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Driver_NewDriver_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var driver = new Driver
        {
            LicenseNumber = "DL123",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            UserId = Guid.NewGuid()
        };

        // Assert
        driver.IsDeleted.Should().BeFalse();
        driver.DeletedAt.Should().BeNull();
        driver.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Zone_NewZone_ShouldHaveDefaultSoftDeleteValues()
    {
        // Arrange
        var depot = new Depot { Name = "Main Depot" };

        // Act
        var zone = new Zone
        {
            Name = "North Zone",
            Depot = depot,
            DepotId = depot.Id,
            BoundaryGeometry = NetTopologySuite.Geometries.GeometryFactory.Default.CreatePolygon()
        };

        // Assert
        zone.IsDeleted.Should().BeFalse();
        zone.DeletedAt.Should().BeNull();
        zone.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Vehicle_NewVehicle_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var vehicle = Vehicle.Create(
            "ABC-123",
            Domain.Enums.VehicleType.Van,
            parcelCapacity: 50,
            weightCapacityKg: 1000m);

        // Assert
        vehicle.IsDeleted.Should().BeFalse();
        vehicle.DeletedAt.Should().BeNull();
        vehicle.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Address_NewAddress_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var address = new Address
        {
            Street1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };

        // Assert
        address.IsDeleted.Should().BeFalse();
        address.DeletedAt.Should().BeNull();
        address.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Permission_NewPermission_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var permission = Permission.Create(
            "users.read",
            "Read Users",
            Domain.Enums.PermissionModule.Users,
            Domain.Enums.PermissionScope.All);

        // Assert
        permission.IsDeleted.Should().BeFalse();
        permission.DeletedAt.Should().BeNull();
        permission.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Parcel_NewParcel_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var parcel = Parcel.Create("Test parcel", Domain.Enums.ServiceType.Standard);

        // Assert
        parcel.IsDeleted.Should().BeFalse();
        parcel.DeletedAt.Should().BeNull();
        parcel.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void TrackingEvent_NewTrackingEvent_ShouldHaveDefaultSoftDeleteValues()
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
        var parcel = Parcel.Create("Test", Domain.Enums.ServiceType.Standard);

        // Act
        var trackingEvent = new TrackingEvent
        {
            ParcelId = parcel.Id,
            Parcel = parcel,
            Timestamp = DateTimeOffset.UtcNow,
            EventType = Domain.Enums.EventType.LabelCreated
        };

        // Assert
        trackingEvent.IsDeleted.Should().BeFalse();
        trackingEvent.DeletedAt.Should().BeNull();
        trackingEvent.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void DayOff_NewDayOff_ShouldHaveDefaultSoftDeleteValues()
    {
        // Arrange
        var depot = new Depot { Name = "Main Depot" };
        var driver = new Driver
        {
            LicenseNumber = "DL123",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            UserId = Guid.NewGuid()
        };

        // Act
        var dayOff = new DayOff
        {
            Date = DateTimeOffset.UtcNow,
            DriverId = driver.Id,
            Driver = driver
        };

        // Assert
        dayOff.IsDeleted.Should().BeFalse();
        dayOff.DeletedAt.Should().BeNull();
        dayOff.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void ShiftSchedule_NewShiftSchedule_ShouldHaveDefaultSoftDeleteValues()
    {
        // Arrange
        var depot = new Depot { Name = "Main Depot" };
        var driver = new Driver
        {
            LicenseNumber = "DL123",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            UserId = Guid.NewGuid()
        };

        // Act
        var shiftSchedule = new ShiftSchedule
        {
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            DriverId = driver.Id,
            Driver = driver
        };

        // Assert
        shiftSchedule.IsDeleted.Should().BeFalse();
        shiftSchedule.DeletedAt.Should().BeNull();
        shiftSchedule.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void ParcelContentItem_NewParcelContentItem_ShouldHaveDefaultSoftDeleteValues()
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
        var parcel = Parcel.Create("Test", Domain.Enums.ServiceType.Standard);

        // Act
        var contentItem = new ParcelContentItem
        {
            ParcelId = parcel.Id,
            Parcel = parcel,
            HsCode = "1234.56",
            Description = "Test item",
            Quantity = 1,
            UnitValue = 10.00m,
            Currency = "USD",
            Weight = 0.5m,
            WeightUnit = Domain.Enums.WeightUnit.Kg,
            CountryOfOrigin = "US"
        };

        // Assert
        contentItem.IsDeleted.Should().BeFalse();
        contentItem.DeletedAt.Should().BeNull();
        contentItem.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void ParcelWatcher_NewParcelWatcher_ShouldHaveDefaultSoftDeleteValues()
    {
        // Act
        var parcelWatcher = new ParcelWatcher
        {
            Email = "watcher@example.com",
            Name = "Test Watcher"
        };

        // Assert
        parcelWatcher.IsDeleted.Should().BeFalse();
        parcelWatcher.DeletedAt.Should().BeNull();
        parcelWatcher.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void DeliveryConfirmation_NewDeliveryConfirmation_ShouldHaveDefaultSoftDeleteValues()
    {
        // Arrange
        var parcel = Parcel.Create("Test", Domain.Enums.ServiceType.Standard);

        // Act
        var confirmation = new DeliveryConfirmation
        {
            ParcelId = parcel.Id,
            Parcel = parcel,
            ReceivedBy = "John Doe",
            DeliveredAt = DateTimeOffset.UtcNow
        };

        // Assert
        confirmation.IsDeleted.Should().BeFalse();
        confirmation.DeletedAt.Should().BeNull();
        confirmation.DeletedBy.Should().BeNull();
    }
}