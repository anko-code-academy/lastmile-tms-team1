using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Bins.Commands.DeleteBin;

public class DeleteBinHandler(IAppDbContext dbContext) : IRequestHandler<DeleteBinCommand, bool>
{
    public async Task<bool> Handle(DeleteBinCommand request, CancellationToken cancellationToken)
    {
        var bin = await dbContext.Bins
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (bin is null)
            return false;

        dbContext.Bins.Remove(bin);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
