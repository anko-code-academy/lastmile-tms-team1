using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Parcels.Commands.ScanParcel;

public class ScanParcelCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<ScanParcelCommand, ScanParcelResult>
{
    public async Task<ScanParcelResult> Handle(ScanParcelCommand request, CancellationToken cancellationToken)
    {
        var parcel = await dbContext.Parcels
            .Include(p => p.Zone)
            .Include(p => p.Bin)
            .Include(p => p.RouteStop!).ThenInclude(rs => rs.Route)
            .FirstOrDefaultAsync(p => p.TrackingNumber == request.TrackingNumber, cancellationToken)
            ?? throw new KeyNotFoundException($"Parcel with tracking number '{request.TrackingNumber}' not found.");

        if (request.NewStatus == ParcelStatus.Cancelled)
            throw new InvalidOperationException("Use CancelParcelCommand to cancel a parcel.");

        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated");

        if (request.RouteId.HasValue)
        {
            var routeId = parcel.RouteStop?.RouteId;
            if (routeId != request.RouteId)
                throw new InvalidOperationException(
                    $"Parcel '{parcel.TrackingNumber}' does not belong to the selected route.");
        }

        var previousStatus = parcel.Status;
        parcel.TransitionTo(request.NewStatus);

        if (request.NewStatus == ParcelStatus.FailedAttempt)
            parcel.DeliveryAttempts++;

        var trackingEvent = new TrackingEvent
        {
            ParcelId = parcel.Id,
            Timestamp = DateTimeOffset.UtcNow,
            EventType = MapToEventType(request.NewStatus),
            Description = request.Description,
            Operator = userId
        };

        dbContext.TrackingEvents.Add(trackingEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ScanParcelResult(
            parcel.Id,
            parcel.TrackingNumber,
            previousStatus,
            parcel.Status,
            parcel.Zone?.Name,
            parcel.Bin?.Label,
            parcel.RouteStop?.Route?.Name
        );
    }

    private static EventType MapToEventType(ParcelStatus status) => status switch
    {
        ParcelStatus.ReceivedAtDepot => EventType.ArrivedAtFacility,
        ParcelStatus.Sorted => EventType.DepartedFacility,
        ParcelStatus.Staged => EventType.HeldAtFacility,
        ParcelStatus.Loaded => EventType.InTransit,
        ParcelStatus.OutForDelivery => EventType.OutForDelivery,
        ParcelStatus.Delivered => EventType.Delivered,
        ParcelStatus.FailedAttempt => EventType.DeliveryAttempted,
        ParcelStatus.ReturnedToDepot => EventType.Returned,
        ParcelStatus.Exception => EventType.Exception,
        _ => EventType.Exception
    };
}
