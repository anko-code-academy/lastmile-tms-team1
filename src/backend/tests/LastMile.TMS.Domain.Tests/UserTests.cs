using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_ShouldInitializeUserWithCorrectDefaults()
    {
        // Act
        var user = User.Create("John", "Doe", "john.doe@lastmile.com", phone: "+1234567890");

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john.doe@lastmile.com");
        user.Phone.Should().Be("+1234567890");
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Act
        var user1 = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");
        var user2 = User.Create("Jane", "Doe", "jane@lastmile.com", phone: "+0987654321");

        // Assert
        user1.Id.Should().NotBeEmpty();
        user2.Id.Should().NotBeEmpty();
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetStatusToActive()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_WhenSuspended_ShouldSetStatusToActive()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");
        user.Suspend();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetStatusToInactive()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");

        // Act
        user.Deactivate();

        // Exception
        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void AssignToZone_ShouldSetZoneId()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");
        var zoneId = Guid.NewGuid();

        // Act
        user.ZoneId = zoneId;

        // Assert
        user.ZoneId.Should().Be(zoneId);
    }

    [Fact]
    public void AssignToDepot_ShouldSetDepotId()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");
        var depotId = Guid.NewGuid();

        // Act
        user.DepotId = depotId;

        // Assert
        user.DepotId.Should().Be(depotId);
    }

    [Fact]
    public void SetPasswordHash_ShouldUpdatePasswordHash()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@lastmile.com", phone: "+1234567890");
        var passwordHash = "hashed_password_here";

        // Act
        user.SetPasswordHash(passwordHash);

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFirstName_ShouldThrow(string? firstName)
    {
        // Act
        var act = () => User.Create(firstName!, "Doe", "john@lastmile.com", phone: "+1234567890");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidLastName_ShouldThrow(string? lastName)
    {
        // Act
        var act = () => User.Create("John", lastName!, "john@lastmile.com", phone: "+1234567890");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public void Create_WithInvalidEmail_ShouldThrow(string? email)
    {
        // Act
        var act = () => User.Create("John", "Doe", email!, "+1234567890");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}