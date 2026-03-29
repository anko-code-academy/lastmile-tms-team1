using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Commands.DeleteDepot;

public record DeleteDepotCommand(Guid Id) : IRequest<bool>;
