using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Domain.Extensions;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Domain.Entities;

public class Route : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public RouteStatus Status { get; set; } = RouteStatus.Draft;
    public DateTime PlannedStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public int TotalParcelCount { get; set; }

    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    public ICollection<VehicleJourney> VehicleJourneys { get; set; } = [];
    public ICollection<RouteStop> RouteStops { get; set; } = [];

    // Status transition validation
    public bool CanTransitionTo(RouteStatus newStatus)
    {
        return newStatus switch
        {
            RouteStatus.Draft => false, // Cannot go back to Draft
            RouteStatus.InProgress => Status == RouteStatus.Draft,
            RouteStatus.Completed => Status == RouteStatus.InProgress,

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

    public void AssignDriver(Guid? driverId)
    {
        if (Status != RouteStatus.Draft)
        {
            throw new InvalidOperationException("Driver can only be assigned to routes in Draft status.");
        }

        DriverId = driverId;
    }

    public void RecalculateTotals()
    {
        TotalParcelCount = RouteStops.Sum(s => s.Parcels.Count);
    }

    public void RecalculateDistance(Point? depotGeoLocation)
    {
        if (depotGeoLocation is null || RouteStops.Count == 0)
        {
            TotalDistanceKm = 0;
            return;
        }

        var orderedStops = RouteStops
            .OrderBy(s => s.SequenceNumber)
            .Where(s => s.GeoLocation is not null)
            .Select(s => s.GeoLocation!)
            .ToList();

        if (orderedStops.Count == 0)
        {
            TotalDistanceKm = 0;
            return;
        }

        var totalMeters = 0.0;

        // Depot to first stop
        totalMeters += depotGeoLocation.HaversineDistanceMeters(orderedStops[0]);

        // Between consecutive stops
        for (var i = 1; i < orderedStops.Count; i++)
        {
            totalMeters += orderedStops[i - 1].HaversineDistanceMeters(orderedStops[i]);
        }

        TotalDistanceKm = (decimal)Math.Round(totalMeters / 1000.0, 2);
    }
}
