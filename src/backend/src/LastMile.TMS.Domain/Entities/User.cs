using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Domain.Entities;

public class User : IdentityUser<Guid>, IBaseAuditableEntity
{
    // Override to get UUIDv7 auto-generation (like BaseEntity does)
    public override Guid Id { get; set; } = Guid.CreateVersion7();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public UserStatus Status { get; set; }

    // Calculated property for easy filtering
    public bool IsActive => Status == UserStatus.Active;

    // Backwards compatibility alias for PhoneNumber
    public string? Phone => PhoneNumber;

    // System admin flag (cannot be deactivated or modified by other admins)
    public bool IsSystemAdmin { get; set; }

    // IBaseAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Role assignment - plain EF Core properties
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public Role? Role { get; set; }

    // Zone assignment - plain EF Core properties
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public Zone? Zone { get; set; }

    // Depot assignment - plain EF Core properties
    public Guid? DepotId { get; set; }
    public string? DepotName { get; set; }
    public Depot? Depot { get; set; }

    // Factory method
    public static User Create(
        string firstName,
        string lastName,
        string email,
        string? userName = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            UserName = userName?.Trim() ?? email.ToLowerInvariant().Trim(),
            PhoneNumber = phone?.Trim(),
            Status = UserStatus.Active
        };

        return user;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Domain methods for status
    public void Activate() => Status = UserStatus.Active;
    public void Deactivate() => Status = UserStatus.Inactive;
    public void Suspend() => Status = UserStatus.Suspended;
    public void MarkAsSystemAdmin() => IsSystemAdmin = true;

    // Domain methods for validation
    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void UpdatePhone(string? phone)
    {
        PhoneNumber = phone?.Trim();
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Email = email.ToLowerInvariant().Trim();
        UserName = email.ToLowerInvariant().Trim();
    }

    // Set password hash (PasswordHash is inherited from IdentityUser but with protected setter)
    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
