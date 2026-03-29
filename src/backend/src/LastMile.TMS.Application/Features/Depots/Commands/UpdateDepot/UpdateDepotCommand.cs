using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;
using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;

public record UpdateAddressInput(
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

public record UpdateDepotCommand(
    Guid Id,
    string Name,
    UpdateAddressInput Address,
    List<DailyOperatingHoursInput>? OperatingHours,
    bool IsActive) : IRequest<DepotResult>;
