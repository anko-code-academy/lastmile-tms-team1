using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Entities;

public class RouteStop : BaseAuditableEntity
{
    public int SequenceNumber { get; set; }
    public RouteStopStatus Status { get; set; } = RouteStopStatus.Pending;
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public int EstimatedServiceMinutes { get; set; }
    public string? AccessInstructions { get; set; }

    // Building-level address (display only)
    public string Street1 { get; set; } = string.Empty;
    public Point? GeoLocation { get; set; }

    // Foreign keys
    public Guid RouteId { get; set; }

    // Navigation properties
    public Route? Route { get; set; }
    public ICollection<Parcel> Parcels { get; set; } = [];

    // Status transition validation
    public bool CanTransitionTo(RouteStopStatus newStatus)
    {
        return newStatus switch
        {
            RouteStopStatus.Pending => false,
            RouteStopStatus.Arrived => Status == RouteStopStatus.Pending,
            RouteStopStatus.Completed => Status == RouteStopStatus.Arrived,
            RouteStopStatus.Skipped => Status == RouteStopStatus.Pending,

            _ => false
        };
    }

    public void TransitionTo(RouteStopStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
        }

        Status = newStatus;

        if (newStatus == RouteStopStatus.Arrived)
        {
            ArrivalTime = DateTime.UtcNow;
        }
        else if (newStatus is RouteStopStatus.Completed or RouteStopStatus.Skipped)
        {
            DepartureTime = DateTime.UtcNow;
        }
    }
}
