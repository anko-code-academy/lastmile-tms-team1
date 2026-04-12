using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Manifests.Commands.ReceiveParcel;

public class ReceiveParcelCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<ReceiveParcelCommand, ReceiveParcelResult>
{
    public async Task<ReceiveParcelResult> Handle(ReceiveParcelCommand request, CancellationToken cancellationToken)
    {
        var manifest = await dbContext.Manifests
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == request.ManifestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Manifest with ID {request.ManifestId} not found.");

        if (manifest.Status == ManifestStatus.Open)
            manifest.StartReceiving();

        if (manifest.Status != ManifestStatus.Receiving)
            throw new InvalidOperationException($"Cannot receive parcels for manifest in status {manifest.Status}");

        var parcel = await dbContext.Parcels
            .FirstOrDefaultAsync(p => p.TrackingNumber == request.TrackingNumber, cancellationToken)
            ?? throw new KeyNotFoundException($"Parcel with tracking number '{request.TrackingNumber}' not found.");

        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated");

        // Non-Registered parcels go to Exception
        if (parcel.Status != ParcelStatus.Registered)
        {
            var previousStatus = parcel.Status;
            parcel.TransitionTo(ParcelStatus.Exception);

            dbContext.TrackingEvents.Add(new TrackingEvent
            {
                ParcelId = parcel.Id,
                Timestamp = DateTimeOffset.UtcNow,
                EventType = EventType.Exception,
                Description = $"Invalid scan: parcel is in {previousStatus} status, expected Registered",
                Operator = userId
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            return new ReceiveParcelResult(
                parcel.Id,
                parcel.TrackingNumber,
                ParcelStatus.Exception,
                null
            );
        }

        var matchingItem = manifest.Items
            .FirstOrDefault(i => i.TrackingNumber == request.TrackingNumber && i.Status == ManifestItemStatus.Expected);

        ManifestItemStatus itemStatus;
        bool isUnexpected;

        if (matchingItem is not null)
        {
            matchingItem.MarkReceived(parcel.Id);
            itemStatus = ManifestItemStatus.Received;
            isUnexpected = false;
        }
        else
        {
            var unexpectedItem = ManifestItem.Create(manifest.Id, request.TrackingNumber);
            unexpectedItem.MarkUnexpected(parcel.Id);
            manifest.Items.Add(unexpectedItem);
            itemStatus = ManifestItemStatus.Unexpected;
            isUnexpected = true;
        }

        var targetStatus = isUnexpected ? ParcelStatus.Exception : ParcelStatus.ReceivedAtDepot;
        parcel.TransitionTo(targetStatus);

        var trackingEvent = new TrackingEvent
        {
            ParcelId = parcel.Id,
            Timestamp = DateTimeOffset.UtcNow,
            EventType = isUnexpected ? EventType.Exception : EventType.ArrivedAtFacility,
            Description = isUnexpected
                ? "Parcel not on manifest"
                : "Received at depot (manifest match)",
            Operator = userId
        };

        dbContext.TrackingEvents.Add(trackingEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ReceiveParcelResult(
            parcel.Id,
            parcel.TrackingNumber,
            parcel.Status,
            itemStatus
        );
    }
}
