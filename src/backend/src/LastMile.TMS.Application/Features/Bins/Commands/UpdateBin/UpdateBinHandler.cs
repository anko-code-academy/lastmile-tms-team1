using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Commands;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Bins.Commands.UpdateBin;

public class UpdateBinHandler(IAppDbContext dbContext) : IRequestHandler<UpdateBinCommand, BinResult>
{
    public async Task<BinResult> Handle(UpdateBinCommand request, CancellationToken cancellationToken)
    {
        var bin = await dbContext.Bins
            .Include(b => b.Zone)
            .ThenInclude(z => z.Depot)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Bin with ID {request.Id} not found.");

        var zone = await dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == request.ZoneId, cancellationToken)
            ?? throw new InvalidOperationException($"Zone with ID {request.ZoneId} not found.");

        bin.Description = request.Description;
        bin.Aisle = request.Aisle;
        bin.Slot = request.Slot;
        bin.Capacity = request.Capacity;
        bin.IsActive = request.IsActive;
        bin.ZoneId = request.ZoneId;

        // Re-generate label if aisle or slot changed
        var depotNumber = zone.Depot.Name.Length > 0 ? zone.Depot.Name[0].ToString() : "1";
        var zoneLetter = zone.Name.Length > 0 ? zone.Name[0].ToString().ToUpperInvariant() : "X";
        bin.SetLabel(depotNumber, zoneLetter);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new BinResult(
            bin.Id,
            bin.Label,
            bin.Description,
            bin.Aisle,
            bin.Slot,
            bin.Capacity,
            bin.IsActive,
            bin.ZoneId,
            zone.Name,
            bin.CreatedAt);
    }
}
