using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace LastMile.TMS.Application.Features.Vehicles.Commands;

public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteVehicleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _context.Vehicles.Where(v => v.Id == request.Id), cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with ID {request.Id} not found");

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
