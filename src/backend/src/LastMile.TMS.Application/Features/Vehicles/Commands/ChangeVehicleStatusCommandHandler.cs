using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Vehicles;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace LastMile.TMS.Application.Features.Vehicles.Commands;

public class ChangeVehicleStatusCommandHandler : IRequestHandler<ChangeVehicleStatusCommand, VehicleDto>
{
    private readonly IAppDbContext _context;

    public ChangeVehicleStatusCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleDto> Handle(ChangeVehicleStatusCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _context.Vehicles.Include(v => v.Depot).Where(v => v.Id == request.Id), cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with ID {request.Id} not found");

        vehicle.TransitionTo(request.NewStatus);
        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleDto
        {
            Id = vehicle.Id,
            RegistrationPlate = vehicle.RegistrationPlate,
            Type = vehicle.Type,
            ParcelCapacity = vehicle.ParcelCapacity,
            WeightCapacityKg = vehicle.WeightCapacityKg,
            Status = vehicle.Status,
            DepotId = vehicle.DepotId,
            DepotName = vehicle.Depot?.Name,
            CreatedAt = vehicle.CreatedAt
        };
    }
}
