using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Features.Routes;

internal static class RouteMapping
{
    public static RouteDto ToDto(this Route route) => new()
    {
        Id = route.Id,
        Name = route.Name,
        Status = route.Status,
        PlannedStartTime = route.PlannedStartTime,
        ActualStartTime = route.ActualStartTime,
        ActualEndTime = route.ActualEndTime,
        TotalDistanceKm = route.TotalDistanceKm,
        TotalParcelCount = route.TotalParcelCount,
        VehicleId = route.VehicleId,
        VehiclePlate = route.Vehicle?.RegistrationPlate,
        DriverId = route.DriverId,
        DriverName = route.Driver != null
            ? $"{route.Driver.User.FirstName} {route.Driver.User.LastName}"
            : null,
        ZoneId = route.ZoneId,
        ZoneName = route.Zone?.Name,
        EstimatedStopCount = route.RouteStops.Count,
        Stops = route.RouteStops
            .OrderBy(s => s.SequenceNumber)
            .Select(s => new RouteStopDto
            {
                Id = s.Id,
                SequenceNumber = s.SequenceNumber,
                Status = s.Status,
                ArrivalTime = s.ArrivalTime,
                DepartureTime = s.DepartureTime,
                EstimatedServiceMinutes = s.EstimatedServiceMinutes,
                AccessInstructions = s.AccessInstructions,
                Street1 = s.Street1,
                ParcelCount = s.Parcels.Count,
                Parcels = s.Parcels.Select(p => new RouteStopParcelDto
                {
                    Id = p.Id,
                    TrackingNumber = p.TrackingNumber,
                    Status = p.Status
                }).ToList()
            }).ToList(),
        CreatedAt = route.CreatedAt
    };
}
