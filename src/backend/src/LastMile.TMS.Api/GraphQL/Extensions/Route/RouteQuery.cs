using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Persistence;
using RouteEntity = LastMile.TMS.Domain.Entities.Route;
using LastMile.TMS.Domain.Entities;

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
        return context.Routes;
    }

    [Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
    [UseProjection]
    [UseFirstOrDefault]
    public IQueryable<RouteEntity> GetRoute(AppDbContext context, Guid id)
    {
        return context.Routes.Where(r => r.Id == id);
    }
}
