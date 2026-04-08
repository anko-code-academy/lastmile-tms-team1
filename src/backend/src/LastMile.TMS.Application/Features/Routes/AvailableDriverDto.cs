using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes;

public record AvailableDriverDto(
    Guid Id,
    string Name,
    ShiftInfoDto? Shift,
    List<DriverRouteSummaryDto> AssignedRoutes);

public record ShiftInfoDto(TimeOnly OpenTime, TimeOnly CloseTime);

public record DriverRouteSummaryDto(Guid Id, string Name, RouteStatus Status);
