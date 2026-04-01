using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Features.Vehicles;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using VehicleEntity = LastMile.TMS.Domain.Entities.Vehicle;

namespace LastMile.TMS.Api.GraphQL.Extensions.Vehicle;

[ExtendObjectType(typeof(Query))]
public class VehicleQuery
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<VehicleEntity> GetVehicles(AppDbContext context)
    {
        return context.Vehicles.AsNoTracking();
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<VehicleEntity> GetVehicle(AppDbContext context, Guid id)
    {
        return context.Vehicles.AsNoTracking().Where(v => v.Id == id);
    }

    // GetVehicleHistory uses manual aggregation (Sum, Count, Distinct) that cannot be
    // expressed through UseFiltering/UseProjection. The aggregations require loading
    // journeys into memory and computing totals, which is not supported by simple projection.
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<VehicleHistoryDto?> GetVehicleHistory(
        AppDbContext context,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicleExists = await context.Vehicles.AnyAsync(v => v.Id == id, cancellationToken);
        if (!vehicleExists)
            return null;

        var journeys = await context.VehicleJourneys
            .Where(vj => vj.VehicleId == id)
            .Include(vj => vj.Route)
            .OrderByDescending(vj => vj.StartTime)
            .ToListAsync(cancellationToken);

        var totalMileageKm = journeys.Sum(j => j.DistanceKm);
        var totalRoutesCompleted = journeys
            .Where(j => j.Route != null && j.Route.Status == Domain.Enums.RouteStatus.Completed)
            .Select(j => j.RouteId)
            .Distinct()
            .Count();

        var routes = journeys
            .Where(j => j.Route != null)
            .Select(j => new RouteHistoryDto
            {
                RouteId = j.RouteId,
                RouteName = j.Route!.Name,
                CompletedAt = j.Route.ActualEndTime ?? j.EndTime ?? DateTime.MinValue,
                DistanceKm = j.DistanceKm,
                ParcelCount = j.Route.TotalParcelCount
            })
            .ToList();

        var vehicle = await context.Vehicles
            .Where(v => v.Id == id)
            .Select(v => new
            {
                v.Id,
                v.RegistrationPlate,
                v.Type
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new VehicleHistoryDto
        {
            Id = vehicle!.Id,
            RegistrationPlate = vehicle.RegistrationPlate,
            Type = vehicle.Type,
            TotalMileageKm = totalMileageKm,
            TotalRoutesCompleted = totalRoutesCompleted,
            Routes = routes
        };
    }
}
