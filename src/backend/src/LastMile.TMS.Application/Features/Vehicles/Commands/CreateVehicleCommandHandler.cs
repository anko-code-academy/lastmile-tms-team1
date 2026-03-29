using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Vehicles;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Vehicles.Commands;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, VehicleDto>
{
    private readonly IAppDbContext _context;

    public CreateVehicleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = Vehicle.Create(
            request.RegistrationPlate,
            request.Type,
            request.ParcelCapacity,
            request.WeightCapacityKg);

        if (request.DepotId.HasValue)
        {
            vehicle.DepotId = request.DepotId;
        }

        _context.Vehicles.Add(vehicle);
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
