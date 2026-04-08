using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class AddParcelsToRouteCommandHandler(IAppDbContext context) : IRequestHandler<AddParcelsToRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(AddParcelsToRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .Include(r => r.RouteStops).ThenInclude(s => s.Parcels)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .Include(r => r.Zone)
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.RouteId} not found.");
        }

        if (route.Status != RouteStatus.Draft)
        {
            throw new InvalidOperationException("Parcels can only be added to routes in Draft status.");
        }

        var parcels = await context.Parcels
            .Include(p => p.RecipientAddress)
            .Where(p => request.ParcelIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (parcels.Count != request.ParcelIds.Count)
        {
            var foundIds = parcels.Select(p => p.Id).ToHashSet();
            var missingIds = request.ParcelIds.Where(id => !foundIds.Contains(id)).ToList();
            throw new InvalidOperationException($"Parcels not found: {string.Join(", ", missingIds)}");
        }

        // Check parcels are not already assigned to another active route
        var alreadyAssigned = parcels.Where(p => p.RouteStopId.HasValue).ToList();
        if (alreadyAssigned.Count > 0)
        {
            throw new InvalidOperationException(
                $"Parcels already assigned to a route: {string.Join(", ", alreadyAssigned.Select(p => p.TrackingNumber))}");
        }

        foreach (var parcel in parcels)
        {
            if (parcel.RecipientAddress?.GeoLocation is null)
            {
                throw new InvalidOperationException(
                    $"Parcel {parcel.TrackingNumber} has no geocoded delivery address.");
            }

            // Find existing stop within 50m proximity using GeoLocation
            var matchingStop = FindMatchingStop(route.RouteStops.ToList(), parcel.RecipientAddress.GeoLocation);

            if (matchingStop is not null)
            {
                parcel.RouteStopId = matchingStop.Id;
                matchingStop.Parcels.Add(parcel);
            }
            else
            {
                var newStop = new RouteStop
                {
                    SequenceNumber = route.RouteStops.Count + 1,
                    Street1 = parcel.RecipientAddress.Street1,
                    GeoLocation = parcel.RecipientAddress.GeoLocation,
                    RouteId = route.Id,
                    Parcels = [parcel]
                };
                parcel.RouteStopId = newStop.Id;
                route.RouteStops.Add(newStop);
                context.RouteStops.Add(newStop);
            }
        }

        route.RecalculateTotals();
        await context.SaveChangesAsync(cancellationToken);

        return route.ToDto();
    }

    private static RouteStop? FindMatchingStop(List<RouteStop> stops, NetTopologySuite.Geometries.Point location)
    {
        const double thresholdMeters = 50.0;

        foreach (var stop in stops)
        {
            if (stop.GeoLocation is null) continue;

            var distance = HaversineDistanceMeters(
                stop.GeoLocation.Y, stop.GeoLocation.X,
                location.Y, location.X);

            if (distance <= thresholdMeters)
            {
                return stop;
            }
        }

        return null;
    }

    private static double HaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6_371_000; // Earth radius in meters
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

}
