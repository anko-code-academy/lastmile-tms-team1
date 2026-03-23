using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Queries.GetDepot;

public record GetDepotQuery(Guid Id) : IRequest<DepotDto?>;
