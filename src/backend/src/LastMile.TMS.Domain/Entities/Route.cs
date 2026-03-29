using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Route : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public RouteStatus Status { get; set; } = RouteStatus.Planned;
    public DateTime PlannedStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public int TotalParcelCount { get; set; }

    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public ICollection<VehicleJourney> VehicleJourneys { get; set; } = [];

    // Status transition validation
    public bool CanTransitionTo(RouteStatus newStatus)
    {
        return newStatus switch
        {
            RouteStatus.Planned => false, // Cannot go back to Planned

            RouteStatus.InProgress => Status == RouteStatus.Planned,
            RouteStatus.Completed => Status == RouteStatus.InProgress,
            RouteStatus.Cancelled => Status == RouteStatus.Planned || Status == RouteStatus.InProgress,

            _ => false
        };
    }

    public void TransitionTo(RouteStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
        }

        Status = newStatus;

        if (newStatus == RouteStatus.InProgress)
        {
            ActualStartTime = DateTime.UtcNow;
        }
        else if (newStatus == RouteStatus.Completed)
        {
            ActualEndTime = DateTime.UtcNow;
        }
    }
}
