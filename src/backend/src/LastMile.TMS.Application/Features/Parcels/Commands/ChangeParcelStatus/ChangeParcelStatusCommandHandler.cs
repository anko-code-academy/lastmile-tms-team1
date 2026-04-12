using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Parcels.Commands.ChangeParcelStatus;

public class ChangeParcelStatusCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<ChangeParcelStatusCommand, ChangeParcelStatusResult>
{
    public async Task<ChangeParcelStatusResult> Handle(ChangeParcelStatusCommand request, CancellationToken cancellationToken)
    {
        var parcel = await dbContext.Parcels
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Parcel with ID {request.Id} not found.");

        if (request.NewStatus == ParcelStatus.Cancelled)
        {
            throw new InvalidOperationException("Use CancelParcelCommand to cancel a parcel.");
        }

        var userName = currentUserService.UserName ?? currentUserService.UserId
            ?? throw new InvalidOperationException("User not authenticated");

        parcel.TransitionTo(request.NewStatus);

        if (request.NewStatus == ParcelStatus.FailedAttempt)
        {
            parcel.DeliveryAttempts++;
        }

        var trackingEvent = new TrackingEvent
        {
            ParcelId = parcel.Id,
            Timestamp = DateTimeOffset.UtcNow,
            EventType = MapToEventType(request.NewStatus),
            Description = request.Description,
            LocationCity = request.LocationCity,
            LocationState = request.LocationState,
            LocationCountry = request.LocationCountry,
            Operator = userName,
            DelayReason = request.ExceptionReason
        };

        dbContext.TrackingEvents.Add(trackingEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeParcelStatusResult(
            parcel.Id,
            parcel.TrackingNumber,
            parcel.Status,
            parcel.DeliveryAttempts);
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
