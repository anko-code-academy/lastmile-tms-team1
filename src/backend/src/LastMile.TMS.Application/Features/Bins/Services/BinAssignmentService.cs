using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Bins.Services;

public interface IBinAssignmentService
{
    Task<bool> AssignToBinAsync(Parcel parcel, CancellationToken cancellationToken);
    void RemoveFromBin(Parcel parcel);
}

public class BinAssignmentService(IAppDbContext dbContext) : IBinAssignmentService
{
    public async Task<bool> AssignToBinAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        if (parcel.ZoneId is null)
            return false;

        // Find active bins in the parcel's zone with available capacity
        var binsInZone = await dbContext.Bins
            .Where(b => b.ZoneId == parcel.ZoneId && b.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var bin in binsInZone)
        {
            var currentCount = await dbContext.Parcels
                .CountAsync(p => p.BinId == bin.Id, cancellationToken);

            if (currentCount < bin.Capacity)
            {
                parcel.BinId = bin.Id;
                return true;
            }
        }

        return false;
    }

    public void RemoveFromBin(Parcel parcel)
    {
        parcel.BinId = null;
    }
}
