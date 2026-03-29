using MediatR;

namespace LastMile.TMS.Application.Features.Zones.Commands.DeleteZone;

public record DeleteZoneCommand(Guid Id) : IRequest<bool>;
