using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Manifests.Commands;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Extensions.Manifest;

[ExtendObjectType(typeof(Query))]
public class ManifestQuery
{
    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager,
        Role.RoleNames.WarehouseOperator })]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Domain.Entities.Manifest> GetManifests([Service] IAppDbContext context)
        => context.Manifests.AsNoTracking();

    [Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager,
        Role.RoleNames.WarehouseOperator })]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Domain.Entities.Manifest> GetManifest(
        Guid id, [Service] IAppDbContext context)
        => context.Manifests.AsNoTracking().Where(m => m.Id == id);
}
