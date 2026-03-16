using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? PasswordHash { get; private set; }
    public UserStatus Status { get; private set; }

    // Zone assignment (for drivers/dispatchers)
    public Guid? ZoneId { get; private set; }
    public Zone? Zone { get; private set; }

    // Depot assignment (for warehouse operators)
    public Guid? DepotId { get; private set; }
    public Depot? Depot { get; private set; }

    // Navigation: User roles
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Factory method
    public static User Create(
        string firstName,
        string lastName,
        string email,
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

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            Phone = phone?.Trim(),
            Status = UserStatus.Active
        };
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

    // Domain methods
    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
    }

    public void AssignToZone(Guid zoneId)
    {
        ZoneId = zoneId;
        DepotId = null;
    }

    public void AssignToDepot(Guid depotId)
    {
        DepotId = depotId;
        ZoneId = null;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}