using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class RoleTests
{
    [Fact]
    public void Create_ShouldInitializeRoleWithCorrectValues()
    {
        // Act
        var role = Role.Create("Admin", "Full system access");

        // Assert
        role.Name.Should().Be("Admin");
        role.Description.Should().Be("Full system access");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Act
        var role1 = Role.Create("Admin");
        var role2 = Role.Create("Dispatcher");

        // Assert
        role1.Id.Should().NotBeEmpty();
        role2.Id.Should().NotBeEmpty();
        role1.Id.Should().NotBe(role2.Id);
    }

    [Fact]
    public void Create_ShouldTrimNameAndDescription()
    {
        // Act
        var role = Role.Create("  Admin  ", "  Full access  ");

        // Assert
        role.Name.Should().Be("Admin");
        role.Description.Should().Be("Full access");
    }

    [Fact]
    public void Create_WithoutDescription_ShouldSetNull()
    {
        // Act
        var role = Role.Create("Admin");

        // Assert
        role.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrow(string? name)
    {
        // Act
        var act = () => Role.Create(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateAdmin_ShouldReturnAdminRole()
    {
        // Act
        var role = Role.CreateAdmin();

        // Assert
        role.Name.Should().Be(Role.RoleNames.Admin);
        role.Description.Should().NotBeNull();
    }

    [Fact]
    public void CreateOperationsManager_ShouldReturnOperationsManagerRole()
    {
        // Act
        var role = Role.CreateOperationsManager();

        // Assert
        role.Name.Should().Be(Role.RoleNames.OperationsManager);
        role.Description.Should().NotBeNull();
    }

    [Fact]
    public void CreateDispatcher_ShouldReturnDispatcherRole()
    {
        // Act
        var role = Role.CreateDispatcher();

        // Assert
        role.Name.Should().Be(Role.RoleNames.Dispatcher);
        role.Description.Should().NotBeNull();
    }

    [Fact]
    public void CreateWarehouseOperator_ShouldReturnWarehouseOperatorRole()
    {
        // Act
        var role = Role.CreateWarehouseOperator();

        // Assert
        role.Name.Should().Be(Role.RoleNames.WarehouseOperator);
        role.Description.Should().NotBeNull();
    }

    [Fact]
    public void CreateDriver_ShouldReturnDriverRole()
    {
        // Act
        var role = Role.CreateDriver();

        // Assert
        role.Name.Should().Be(Role.RoleNames.Driver);
        role.Description.Should().NotBeNull();
    }

    [Fact]
    public void UserRoles_ShouldInitializeAsEmptyCollection()
    {
        // Act
        var role = Role.Create("Admin");

        // Assert
        role.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void RolePermissions_ShouldInitializeAsEmptyCollection()
    {
        // Act
        var role = Role.Create("Admin");

        // Assert
        role.RolePermissions.Should().BeEmpty();
    }
}