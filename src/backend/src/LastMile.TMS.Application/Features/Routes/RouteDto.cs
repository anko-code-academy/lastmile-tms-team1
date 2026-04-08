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
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public int EstimatedStopCount { get; set; }
    public ICollection<RouteStopDto> Stops { get; set; } = [];
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
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
}

public class RouteStopDto
{
    public Guid Id { get; set; }
    public int SequenceNumber { get; set; }
    public RouteStopStatus Status { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public int EstimatedServiceMinutes { get; set; }
    public string? AccessInstructions { get; set; }
    public string Street1 { get; set; } = string.Empty;
    public int ParcelCount { get; set; }
    public ICollection<RouteStopParcelDto> Parcels { get; set; } = [];
}

public class RouteStopParcelDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public ParcelStatus Status { get; set; }
}
