using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Depots.Commands.DeleteDepot;

public class DeleteDepotHandler(IAppDbContext dbContext) : IRequestHandler<DeleteDepotCommand, bool>
{
    public async Task<bool> Handle(DeleteDepotCommand request, CancellationToken cancellationToken)
    {
        var depot = await dbContext.Depots
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Depot with ID {request.Id} not found.");

        depot.IsDeleted = true;
        depot.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
