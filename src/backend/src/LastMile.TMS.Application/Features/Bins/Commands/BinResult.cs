namespace LastMile.TMS.Application.Features.Bins.Commands;

public record BinResult(
    Guid Id,
    string Label,
    string? Description,
    int Aisle,
    int Slot,
    int Capacity,
    bool IsActive,
    Guid ZoneId,
    string ZoneName,
    DateTimeOffset CreatedAt);
