using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Vehicles.Commands;

public record ChangeVehicleStatusCommand(
    Guid Id,
    VehicleStatus NewStatus) : IRequest<VehicleDto>;
