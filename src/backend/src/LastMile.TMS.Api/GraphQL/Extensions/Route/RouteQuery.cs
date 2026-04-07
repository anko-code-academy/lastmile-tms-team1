using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using RouteEntity = LastMile.TMS.Domain.Entities.Route;

namespace LastMile.TMS.Api.GraphQL.Extensions.Route;

[ExtendObjectType(typeof(Query))]
public class RouteQuery
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    [UsePaging(IncludeTotalCount = true, MaxPageSize = 100)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<RouteEntity> GetRoutes(AppDbContext context)
    {
        return context.Routes.AsNoTracking();
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<RouteEntity> GetRoute(AppDbContext context, Guid id)
    {
        return context.Routes.AsNoTracking().Where(r => r.Id == id);
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager, Role.RoleNames.Dispatcher])]
    public IQueryable<AvailableDriverDto> GetAvailableDrivers(
        AppDbContext context,
        DateTime date)
    {
        var dayOfWeek = date.DayOfWeek;
        var dateOnly = DateOnly.FromDateTime(date);
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return context.Drivers
            .Include(d => d.User)
            .Include(d => d.ShiftSchedules)
            .Include(d => d.DaysOff)
            .Include(d => d.Routes)
            .Where(d => d.User.Status == UserStatus.Active)
            .Where(d => !d.DaysOff.Any(dto => DateOnly.FromDateTime(dto.Date.DateTime) == dateOnly))
            .Where(d => d.ShiftSchedules.Any(s => s.DayOfWeek == dayOfWeek && s.DriverId == d.Id))
            .Select(d => new AvailableDriverDto(
                d.Id,
                d.User.FirstName + " " + d.User.LastName,
                d.ShiftSchedules
                    .Where(s => s.DayOfWeek == dayOfWeek && s.DriverId == d.Id)
                    .Select(s => new ShiftInfoDto(s.OpenTime, s.CloseTime))
                    .FirstOrDefault(),
                d.Routes
                    .Where(r => r.PlannedStartTime >= startOfDay && r.PlannedStartTime < endOfDay
                        && (r.Status == RouteStatus.Draft || r.Status == RouteStatus.InProgress))
                    .Select(r => new DriverRouteSummaryDto(r.Id, r.Name, r.Status))
                    .ToList()))
            .AsNoTracking();
    }
}
