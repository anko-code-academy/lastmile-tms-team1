using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Zones.Commands.DeleteZone;

public class DeleteZoneHandler(IAppDbContext dbContext) : IRequestHandler<DeleteZoneCommand, bool>
{
    public async Task<bool> Handle(DeleteZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Zone with ID {request.Id} not found.");

        dbContext.Zones.Remove(zone);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
