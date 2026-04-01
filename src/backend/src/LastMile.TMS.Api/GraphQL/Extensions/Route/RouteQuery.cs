using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using RouteEntity = LastMile.TMS.Domain.Entities.Route;

namespace LastMile.TMS.Api.GraphQL.Extensions.Route;

[ExtendObjectType(typeof(Query))]
public class RouteQuery
{
    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<RouteEntity> GetRoutes(AppDbContext context)
    {
        return context.Routes.AsNoTracking();
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<RouteEntity> GetRoute(AppDbContext context, Guid id)
    {
        return context.Routes.AsNoTracking().Where(r => r.Id == id);
    }
}
