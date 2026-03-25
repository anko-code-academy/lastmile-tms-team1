using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Application.Features.Vehicles;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL;

public class Query
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<IReadOnlyList<VehicleSummaryDto>> GetVehicles(
        AppDbContext context,
        VehicleStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Vehicles.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        return await query
            .OrderBy(v => v.RegistrationPlate)
            .Select(v => new VehicleSummaryDto
            {
                Id = v.Id,
                RegistrationPlate = v.RegistrationPlate,
                Type = v.Type,
                Status = v.Status,
                DepotId = v.DepotId
            })
            .ToListAsync(cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<VehicleDto?> GetVehicle(
        AppDbContext context,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (vehicle is null)
            return null;

        return new VehicleDto
        {
            Id = vehicle.Id,
            RegistrationPlate = vehicle.RegistrationPlate,
            Type = vehicle.Type,
            ParcelCapacity = vehicle.ParcelCapacity,
            WeightCapacityKg = vehicle.WeightCapacityKg,
            Status = vehicle.Status,
            DepotId = vehicle.DepotId,
            CreatedAt = vehicle.CreatedAt
        };
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<VehicleHistoryDto?> GetVehicleHistory(
        AppDbContext context,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (vehicle is null)
            return null;

        var journeys = await context.VehicleJourneys
            .Where(vj => vj.VehicleId == id)
            .Include(vj => vj.Route)
            .OrderByDescending(vj => vj.StartTime)
            .ToListAsync(cancellationToken);

        var totalMileageKm = journeys.Sum(j => j.DistanceKm);
        var totalRoutesCompleted = journeys
            .Where(j => j.Route != null && j.Route.Status == RouteStatus.Completed)
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

        return new VehicleHistoryDto
        {
            Id = vehicle.Id,
            RegistrationPlate = vehicle.RegistrationPlate,
            Type = vehicle.Type,
            TotalMileageKm = totalMileageKm,
            TotalRoutesCompleted = totalRoutesCompleted,
            Routes = routes
        };
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<IReadOnlyList<RouteSummaryDto>> GetRoutes(
        AppDbContext context,
        RouteStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Routes.Include(r => r.Vehicle).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query
            .OrderBy(r => r.PlannedStartTime)
            .Select(r => new RouteSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                Status = r.Status,
                PlannedStartTime = r.PlannedStartTime,
                VehicleId = r.VehicleId,
                VehiclePlate = r.Vehicle != null ? r.Vehicle.RegistrationPlate : null
            })
            .ToListAsync(cancellationToken);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    public async Task<RouteDto?> GetRoute(
        AppDbContext context,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var route = await context.Routes
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (route is null)
            return null;

        return new RouteDto
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
            CreatedAt = route.CreatedAt
        };
    }
}
