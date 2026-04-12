using LastMile.TMS.Application.Common.DTOs;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;
using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;

public record UpdateDepotCommand(
    Guid Id,
    string Name,
    AddressInput Address,
    List<DailyOperatingHoursInput>? OperatingHours,
    bool IsActive) : IRequest<DepotResult>;
