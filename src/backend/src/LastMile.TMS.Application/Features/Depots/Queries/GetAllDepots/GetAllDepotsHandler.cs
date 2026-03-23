using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Depots.Queries.GetAllDepots;

public class GetAllDepotsHandler(IAppDbContext dbContext) : IRequestHandler<GetAllDepotsQuery, List<DepotSummaryDto>>
{
    public async Task<List<DepotSummaryDto>> Handle(GetAllDepotsQuery request, CancellationToken cancellationToken)
    {
        var depots = await dbContext.Depots
            .Include(d => d.Address)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return depots.Select(d => new DepotSummaryDto(
            d.Id,
            d.Name,
            d.Address != null ? $"{d.Address.Street1}, {d.Address.City}" : null,
            d.IsActive,
            d.CreatedAt)).ToList();
    }
}

public record DepotSummaryDto(
    Guid Id,
    string Name,
    string? AddressSummary,
    bool IsActive,
    DateTimeOffset CreatedAt);
