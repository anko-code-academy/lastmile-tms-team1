// // using LastMile.TMS.Application.Common.Interfaces;
// // using LastMile.TMS.Domain.Entities;
// // using MediatR;

// // namespace LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

// // public class CreateZoneHandler(IAppDbContext dbContext) : IRequestHandler<CreateZoneCommand, ZoneResult>
// // {
// //     public async Task<ZoneResult> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
// //     {
// //         var depot = await dbContext.Depots.FindAsync([request.DepotId], cancellationToken)
// //             ?? throw new InvalidOperationException($"Depot with ID {request.DepotId} not found.");

// //         var zone = new Zone
// //         {
// //             Name = request.Name,
// //             DepotId = request.DepotId,
// //             IsActive = request.IsActive
// //         };

// //         zone.SetBoundaryFromGeoJson(request.GeoJson);

// //         dbContext.Zones.Add(zone);
// //         await dbContext.SaveChangesAsync(cancellationToken);

// //         return new ZoneResult(zone.Id, zone.Name, zone.DepotId, zone.IsActive, zone.CreatedAt);
// //     }
// // }

// using LastMile.TMS.Application.Common.Interfaces;
// using LastMile.TMS.Domain.Entities;
// using MediatR;
// using Microsoft.EntityFrameworkCore;

// namespace LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

// public class CreateZoneHandler(IAppDbContext dbContext) : IRequestHandler<CreateZoneCommand, ZoneResult>
// {
//     public async Task<ZoneResult> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
//     {
//         // FindAsync bypasses global query filters — use FirstOrDefaultAsync instead
//         var depot = await dbContext.Depots
//             .FirstOrDefaultAsync(d => d.Id == request.DepotId, cancellationToken)
//             ?? throw new InvalidOperationException($"Depot with ID {request.DepotId} not found.");

//         var zone = new Zone
//         {
//             Name = request.Name,
//             DepotId = request.DepotId,
//             IsActive = request.IsActive
//         };

//         zone.SetBoundaryFromGeoJson(request.GeoJson);
//         dbContext.Zones.Add(zone);
//         await dbContext.SaveChangesAsync(cancellationToken);

//         return new ZoneResult(zone.Id, zone.Name, zone.DepotId, zone.IsActive, zone.CreatedAt);
//     }
// }


using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Zones.Commands.CreateZone;

public class CreateZoneHandler(IAppDbContext dbContext) : IRequestHandler<CreateZoneCommand, ZoneResult>
{
    public async Task<ZoneResult> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
    {
        var depot = await dbContext.Depots
            .FirstOrDefaultAsync(d => d.Id == request.DepotId, cancellationToken)
            ?? throw new InvalidOperationException($"Depot with ID {request.DepotId} not found.");

        var zone = new Zone
        {
            Name = request.Name,
            DepotId = request.DepotId,
            IsActive = request.IsActive
        };

        zone.SetBoundaryFromGeoJson(request.GeoJson);
        dbContext.Zones.Add(zone);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ZoneResult(zone.Id, zone.Name, zone.DepotId, zone.IsActive, zone.CreatedAt);
    }
}
