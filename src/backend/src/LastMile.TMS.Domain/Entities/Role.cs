using LastMile.TMS.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Domain.Entities;

public class Role : IdentityRole<Guid>, IBaseAuditableEntity
{
    // Override to get UUIDv7 auto-generation (like BaseEntity does)
    public override Guid Id { get; set; } = Guid.CreateVersion7();

    public string? Description { get; private set; }

    // IBaseAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Navigation: Users with this role
    public ICollection<User> Users { get; private set; } = new List<User>();

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
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
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
