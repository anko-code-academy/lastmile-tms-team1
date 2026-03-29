using MediatR;

namespace LastMile.TMS.Application.Features.Vehicles.Commands;

public record DeleteVehicleCommand(Guid Id) : IRequest<bool>;
