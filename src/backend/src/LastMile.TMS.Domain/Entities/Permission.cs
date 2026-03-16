using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Permission : BaseAuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public PermissionModule Module { get; private set; }
    public PermissionScope Scope { get; private set; }

    // Navigation: Role permissions
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Factory method
    public static Permission Create(
        string code,
        string name,
        PermissionModule module,
        PermissionScope scope,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Permission code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name is required", nameof(name));

        return new Permission
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Module = module,
            Scope = scope
        };
    }
}