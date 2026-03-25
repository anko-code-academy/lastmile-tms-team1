using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Users.Validation;

public static class UserManagementRules
{
    public static async Task<string?> EnsureValidAssignmentsAsync(
        IAppDbContext context,
        Guid? depotId,
        Guid? zoneId)
    {
        // Cannot have both
        if (depotId.HasValue && zoneId.HasValue)
        {
            return "User cannot be assigned to both a zone and a depot";
        }

        // Validate depot exists and is active
        if (depotId.HasValue)
        {
            var depot = await context.Depots
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == depotId.Value);

            if (depot == null)
            {
                return "Depot not found";
            }

            if (depot.IsDeleted)
            {
                return "Depot has been deleted";
            }

            if (!depot.IsActive)
            {
                return "Depot is not active";
            }
        }

        // Validate zone exists, is active, and belongs to correct depot
        if (zoneId.HasValue)
        {
            var zone = await context.Zones
                .IgnoreQueryFilters()
                .Include(z => z.Depot)
                .FirstOrDefaultAsync(z => z.Id == zoneId.Value);

            if (zone == null)
            {
                return "Zone not found";
            }

            if (zone.IsDeleted)
            {
                return "Zone has been deleted";
            }

            if (!zone.IsActive)
            {
                return "Zone is not active";
            }
        }

        return null; // Valid
    }
}
