using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Features.Vehicles;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Extensions.Vehicle;

[ExtendObjectType(typeof(Query))]
public class VehicleQuery
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<VehicleSummaryDto> GetVehicles(AppDbContext context)
    {
        return context.Vehicles
            .OrderBy(v => v.RegistrationPlate)
            .Select(v => new VehicleSummaryDto
            {
                Id = v.Id,
                RegistrationPlate = v.RegistrationPlate,
                Type = v.Type,
                Status = v.Status,
                DepotId = v.DepotId
            });
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseProjection]
    [UseFirstOrDefault]
    public IQueryable<VehicleDto> GetVehicle(AppDbContext context, Guid id)
    {
        return context.Vehicles
            .Where(v => v.Id == id)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                RegistrationPlate = v.RegistrationPlate,
                Type = v.Type,
                ParcelCapacity = v.ParcelCapacity,
                WeightCapacityKg = v.WeightCapacityKg,
                Status = v.Status,
                DepotId = v.DepotId,
                CreatedAt = v.CreatedAt
            });
    }

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
