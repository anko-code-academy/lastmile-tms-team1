using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Role : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Navigation: User roles
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Navigation: Role permissions
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Static role constants
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string OperationsManager = "Operations Manager";
        public const string Dispatcher = "Dispatcher";
        public const string WarehouseOperator = "Warehouse Operator";
        public const string Driver = "Driver";
    }

    // Factory method
    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim()
        };
    }

    // Static factory for standard roles
    public static Role CreateAdmin() => Create(RoleNames.Admin, "Full system access");
    public static Role CreateOperationsManager() => Create(RoleNames.OperationsManager, "Manage operations and dispatch");
    public static Role CreateDispatcher() => Create(RoleNames.Dispatcher, "Assign and track deliveries");
    public static Role CreateWarehouseOperator() => Create(RoleNames.WarehouseOperator, "Manage parcel intake and sorting");
    public static Role CreateDriver() => Create(RoleNames.Driver, "Deliver parcels to customers");
}