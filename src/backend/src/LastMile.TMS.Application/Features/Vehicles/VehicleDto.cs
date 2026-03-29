using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Vehicles;

public class VehicleDto
{
    public Guid Id { get; set; }
    public string RegistrationPlate { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public int ParcelCapacity { get; set; }
    public decimal WeightCapacityKg { get; set; }
    public VehicleStatus Status { get; set; }
    public Guid? DepotId { get; set; }
    public string? DepotName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class VehicleSummaryDto
{
    public Guid Id { get; set; }
    public string RegistrationPlate { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public VehicleStatus Status { get; set; }
    public Guid? DepotId { get; set; }
}

public class VehicleHistoryDto
{
    public Guid Id { get; set; }
    public string RegistrationPlate { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public decimal TotalMileageKm { get; set; }
    public int TotalRoutesCompleted { get; set; }
    public ICollection<RouteHistoryDto> Routes { get; set; } = [];
}

public class RouteHistoryDto
{
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public decimal DistanceKm { get; set; }
    public int ParcelCount { get; set; }
}
