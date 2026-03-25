using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Depots.Queries.GetDepot;

public class GetDepotHandler(IAppDbContext dbContext) : IRequestHandler<GetDepotQuery, DepotDto?>
{
    public async Task<DepotDto?> Handle(GetDepotQuery request, CancellationToken cancellationToken)
    {
        var depot = await dbContext.Depots
            .Include(d => d.Address)
            .Include(d => d.Zones)
            .Include(d => d.ShiftSchedules)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (depot == null) return null;

        return new DepotDto(
            depot.Id,
            depot.Name,
            depot.Address != null ? new AddressDto(
                depot.Address.Street1,
                depot.Address.Street2,
                depot.Address.City,
                depot.Address.State,
                depot.Address.PostalCode,
                depot.Address.CountryCode,
                depot.Address.IsResidential,
                depot.Address.ContactName,
                depot.Address.CompanyName,
                depot.Address.Phone,
                depot.Address.Email) : null,
            depot.ShiftSchedules.Select(h => new DailyOperatingHoursDto(h.DayOfWeek, h.OpenTime, h.CloseTime)).ToList(),
            depot.IsActive,
            depot.CreatedAt,
            depot.LastModifiedAt,
            depot.Zones.Select(z => z.Id).ToList());
    }
}

public record DepotDto(
    Guid Id,
    string Name,
    AddressDto? Address,
    List<DailyOperatingHoursDto> OperatingHours,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt,
    List<Guid> ZoneIds);

public record AddressDto(
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string CountryCode,
    bool IsResidential,
    string? ContactName,
    string? CompanyName,
    string? Phone,
    string? Email);

public record DailyOperatingHoursDto(DayOfWeek DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);