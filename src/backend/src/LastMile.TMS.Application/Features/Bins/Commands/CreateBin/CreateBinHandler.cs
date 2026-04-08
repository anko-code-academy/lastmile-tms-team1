using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Bins.Commands;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Bins.Commands.CreateBin;

public class CreateBinHandler(IAppDbContext dbContext) : IRequestHandler<CreateBinCommand, BinResult>
{
    public async Task<BinResult> Handle(CreateBinCommand request, CancellationToken cancellationToken)
    {
        var zone = await dbContext.Zones
            .Include(z => z.Depot)
            .FirstOrDefaultAsync(z => z.Id == request.ZoneId, cancellationToken)
            ?? throw new InvalidOperationException($"Zone with ID {request.ZoneId} not found.");

        var depotNumber = zone.Depot.Name.Length > 0 ? zone.Depot.Name[0].ToString() : "1"; // first char only to keep label <= 20 chars
        var zoneLetter = zone.Name.Length > 0 ? zone.Name[0].ToString().ToUpperInvariant() : "X";

        var bin = new Bin
        {
            Description = request.Description,
            Aisle = request.Aisle,
            Slot = request.Slot,
            Capacity = request.Capacity,
            IsActive = request.IsActive,
            ZoneId = request.ZoneId
        };

        bin.SetLabel(depotNumber, zoneLetter);
        
        var exists = await dbContext.Bins
            .AnyAsync(b => b.Label == bin.Label, cancellationToken);
            
        if (exists)
        {
            throw new InvalidOperationException($"Bin with label {bin.Label} already exists.");
        }

        dbContext.Bins.Add(bin);
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
