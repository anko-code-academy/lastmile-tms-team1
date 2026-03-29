using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes;

public class RouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public RouteStatus Status { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public int TotalParcelCount { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class RouteSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public RouteStatus Status { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
}
