using LastMile.TMS.Application.Common.DTOs;
using LastMile.TMS.Domain.Common;
using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

public record DailyOperatingHoursInput(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime);

public record CreateDepotCommand(
    string Name,
    AddressInput Address,
    List<DailyOperatingHoursInput>? OperatingHours,
    bool IsActive = true) : IRequest<DepotResult>;

public record DepotResult(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt);
