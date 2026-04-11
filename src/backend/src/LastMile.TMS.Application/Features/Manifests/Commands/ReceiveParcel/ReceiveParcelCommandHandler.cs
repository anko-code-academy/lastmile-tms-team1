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

        if (parcel.Status != ParcelStatus.Registered)
            throw new InvalidOperationException($"Parcel must be in Registered status, but is {parcel.Status}");

        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated");

        var matchingItem = manifest.Items
            .FirstOrDefault(i => i.TrackingNumber == request.TrackingNumber && i.Status == ManifestItemStatus.Expected);

        ManifestItemStatus itemStatus;

        if (matchingItem is not null)
        {
            matchingItem.MarkReceived(parcel.Id);
            itemStatus = ManifestItemStatus.Received;
        }
        else
        {
            var unexpectedItem = ManifestItem.Create(manifest.Id, request.TrackingNumber);
            unexpectedItem.MarkUnexpected(parcel.Id);
            manifest.Items.Add(unexpectedItem);
            dbContext.ManifestItems.Add(unexpectedItem);
            itemStatus = ManifestItemStatus.Unexpected;
        }

        parcel.TransitionTo(ParcelStatus.ReceivedAtDepot);

        var trackingEvent = new TrackingEvent
        {
            ParcelId = parcel.Id,
            Timestamp = DateTimeOffset.UtcNow,
            EventType = EventType.ArrivedAtFacility,
            Description = itemStatus == ManifestItemStatus.Received
                ? "Received at depot (manifest match)"
                : "Received at depot (unexpected)",
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
