using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    // Data scope: None (no restriction), Own (only own data), Department, All
    public DataScope DataScope { get; set; } = DataScope.All;
}

public enum DataScope
{
    None,
    Own,
    Department,
    All
}