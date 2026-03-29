using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class VehicleJourney : BaseAuditableEntity
{
    public Guid RouteId { get; set; }
    public Route? Route { get; set; }

    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal StartMileageKm { get; set; }
    public decimal EndMileageKm { get; set; }

    public decimal DistanceKm => EndMileageKm > 0 && EndMileageKm > StartMileageKm
        ? EndMileageKm - StartMileageKm
        : 0;
}
