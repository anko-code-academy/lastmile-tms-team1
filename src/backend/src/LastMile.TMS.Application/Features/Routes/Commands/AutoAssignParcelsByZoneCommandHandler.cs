using MediatR;
using Microsoft.EntityFrameworkCore;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Routes.Commands;

public class AutoAssignParcelsByZoneCommandHandler(IAppDbContext context, IMediator mediator) : IRequestHandler<AutoAssignParcelsByZoneCommand, RouteDto>
{
    public async Task<RouteDto> Handle(AutoAssignParcelsByZoneCommand request, CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .Include(r => r.RouteStops).ThenInclude(s => s.Parcels)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .Include(r => r.Zone)
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"Route with ID {request.RouteId} not found.");
        }

        if (route.Status != RouteStatus.Draft)
        {
            throw new InvalidOperationException("Parcels can only be auto-assigned to routes in Draft status.");
        }

        if (!route.ZoneId.HasValue)
        {
            throw new InvalidOperationException("Route must have a zone assigned to auto-assign parcels.");
        }

        // Find unassigned Sorted parcels in this zone that are ready for routing
        var unassignedParcels = await context.Parcels
            .Include(p => p.RecipientAddress)
            .Where(p => p.ZoneId == route.ZoneId && !p.RouteStopId.HasValue && p.Status == ParcelStatus.Sorted)
            .ToListAsync(cancellationToken);

        if (unassignedParcels.Count == 0)
        {
            throw new InvalidOperationException("No unassigned parcels found in this zone.");
        }

        var addCommand = new AddParcelsToRouteCommand(request.RouteId, unassignedParcels.Select(p => p.Id).ToList());
        return await mediator.Send(addCommand, cancellationToken);
    }
}
