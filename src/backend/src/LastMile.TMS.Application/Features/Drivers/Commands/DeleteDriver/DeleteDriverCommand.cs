using MediatR;

namespace LastMile.TMS.Application.Features.Drivers.Commands.DeleteDriver;

public record DeleteDriverCommand(Guid Id) : IRequest<bool>;
