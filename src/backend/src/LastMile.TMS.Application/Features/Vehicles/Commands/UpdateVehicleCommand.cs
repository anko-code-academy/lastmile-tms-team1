using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Vehicles.Commands;

public record UpdateVehicleCommand(
    Guid Id,
    string RegistrationPlate,
    VehicleType Type,
    int ParcelCapacity,
    decimal WeightCapacityKg,
    Guid? DepotId) : IRequest<VehicleDto>;
