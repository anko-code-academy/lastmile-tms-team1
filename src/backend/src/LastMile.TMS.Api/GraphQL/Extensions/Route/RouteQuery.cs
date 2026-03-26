using HotChocolate.Authorization;
using HotChocolate.Data;
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
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<RouteSummaryDto> GetRoutes(AppDbContext context)
    {
        return context.Routes
            .OrderBy(r => r.PlannedStartTime)
            .Select(r => new RouteSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                Status = r.Status,
                PlannedStartTime = r.PlannedStartTime,
                VehicleId = r.VehicleId,
                VehiclePlate = r.Vehicle != null ? r.Vehicle.RegistrationPlate : null
            });
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseProjection]
    [UseFirstOrDefault]
    public IQueryable<RouteDto> GetRoute(AppDbContext context, Guid id)
    {
        return context.Routes
            .Where(r => r.Id == id)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                Name = r.Name,
                Status = r.Status,
                PlannedStartTime = r.PlannedStartTime,
                ActualStartTime = r.ActualStartTime,
                ActualEndTime = r.ActualEndTime,
                TotalDistanceKm = r.TotalDistanceKm,
                TotalParcelCount = r.TotalParcelCount,
                VehicleId = r.VehicleId,
                VehiclePlate = r.Vehicle != null ? r.Vehicle.RegistrationPlate : null,
                CreatedAt = r.CreatedAt
            });
    }
}
