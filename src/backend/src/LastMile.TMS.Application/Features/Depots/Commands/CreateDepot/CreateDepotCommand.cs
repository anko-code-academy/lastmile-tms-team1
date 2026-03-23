using LastMile.TMS.Domain.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

public record DailyOperatingHoursInput(DayOfWeek DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);

public record CreateDepotCommand(
    string Name,
    AddressInput? Address,
    List<DailyOperatingHoursInput>? OperatingHours,
    bool IsActive = true) : IRequest<DepotResult>;

public record AddressInput(
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string CountryCode = "US",
    bool IsResidential = false,
    string? ContactName = null,
    string? CompanyName = null,
    string? Phone = null,
    string? Email = null);

public record DepotResult(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt);
