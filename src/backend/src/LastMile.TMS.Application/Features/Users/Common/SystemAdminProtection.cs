using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Features.Users.Common;

public static class SystemAdminProtection
{
    public static void ThrowIfSystemAdmin(User user)
    {
        if (user.IsSystemAdmin)
        {
            throw new InvalidOperationException("System admin cannot be modified or deactivated");
        }
    }
}
