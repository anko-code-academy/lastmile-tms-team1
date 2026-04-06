using MediatR;

namespace LastMile.TMS.Application.Features.Bins.Commands.DeleteBin;

public record DeleteBinCommand(Guid Id) : IRequest<bool>;
