using LastMile.TMS.Application.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Users.Queries.GetUserManagementLookups;

public class GetUserManagementLookupsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetUserManagementLookupsQuery, Result<UserManagementLookupsDto>>
{
    public async Task<Result<UserManagementLookupsDto>> Handle(
        GetUserManagementLookupsQuery request,
        CancellationToken cancellationToken)
    {
        // Get active roles (not deleted)
        var roles = await context.Roles
            .IgnoreQueryFilters()
            .Where(r => !r.IsDeleted)
            .Select(r => new RoleLookupDto(r.Id, r.Name!, r.Description))
            .ToListAsync(cancellationToken);

        // Get active depots (not deleted)
        var depots = await context.Depots
            .IgnoreQueryFilters()
            .Where(d => !d.IsDeleted && d.IsActive)
            .Select(d => new DepotLookupDto(d.Id, d.Name))
            .ToListAsync(cancellationToken);

        // Get active zones (not deleted)
        var zones = await context.Zones
            .IgnoreQueryFilters()
            .Where(z => !z.IsDeleted && z.IsActive)
            .Select(z => new ZoneLookupDto(z.Id, z.Name, z.DepotId))
            .ToListAsync(cancellationToken);

        return Result<UserManagementLookupsDto>.Success(
            new UserManagementLookupsDto(roles, depots, zones));
    }
}
