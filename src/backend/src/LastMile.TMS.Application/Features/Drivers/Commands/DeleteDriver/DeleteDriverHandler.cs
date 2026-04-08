using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.DeleteDriver;

public class DeleteDriverHandler(IAppDbContext dbContext) : IRequestHandler<DeleteDriverCommand, bool>
{
    public async Task<bool> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = await dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Driver with ID {request.Id} not found.");

        driver.IsDeleted = true;
        driver.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
