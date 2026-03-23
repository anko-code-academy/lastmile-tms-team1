using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Queries.GetAllDepots;

public record GetAllDepotsQuery : IRequest<List<DepotSummaryDto>>;
