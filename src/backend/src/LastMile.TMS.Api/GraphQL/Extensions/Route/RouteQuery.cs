using HotChocolate.Authorization;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Extensions.Route;

[ExtendObjectType(typeof(Query))]
public class RouteQuery
{
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
