using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Zones.Queries.GetAllZones;

public class GetAllZonesHandler(IAppDbContext dbContext) : IRequestHandler<GetAllZonesQuery, List<ZoneSummaryDto>>
{
    public async Task<List<ZoneSummaryDto>> Handle(GetAllZonesQuery request, CancellationToken cancellationToken)
    {
        var zones = await dbContext.Zones
            .Include(z => z.Depot)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return zones.Select(z => new ZoneSummaryDto(
            z.Id,
            z.Name,
            z.DepotId,
            z.Depot?.Name,
            z.IsActive,
            z.CreatedAt)).ToList();
    }
}

public record ZoneSummaryDto(
    Guid Id,
    string Name,
    Guid DepotId,
    string? DepotName,
    bool IsActive,
    DateTimeOffset CreatedAt);
