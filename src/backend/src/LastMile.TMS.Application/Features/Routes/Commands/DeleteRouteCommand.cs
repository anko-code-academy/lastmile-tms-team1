using MediatR;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public record DeleteRouteCommand(Guid Id) : IRequest<bool>;
