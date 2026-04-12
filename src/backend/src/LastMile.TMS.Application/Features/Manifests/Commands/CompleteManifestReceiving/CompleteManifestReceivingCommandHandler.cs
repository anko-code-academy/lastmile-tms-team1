using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Manifests.Commands.CompleteManifestReceiving;

public class CompleteManifestReceivingCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<CompleteManifestReceivingCommand, ManifestResult>
{
    public async Task<ManifestResult> Handle(CompleteManifestReceivingCommand request, CancellationToken cancellationToken)
    {
        var manifest = await dbContext.Manifests
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == request.ManifestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Manifest with ID {request.ManifestId} not found.");

        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated");

        var expectedItems = manifest.Items
            .Where(i => i.Status == ManifestItemStatus.Expected)
            .ToList();

        var trackingNumbers = expectedItems.Select(i => i.TrackingNumber).ToList();
        var parcelsByTracking = await dbContext.Parcels
            .Where(p => trackingNumbers.Contains(p.TrackingNumber))
            .ToDictionaryAsync(p => p.TrackingNumber, cancellationToken);

        foreach (var item in expectedItems)
        {
            if (parcelsByTracking.TryGetValue(item.TrackingNumber, out var parcel))

            if (parcel is not null)
            {
                parcel.TransitionTo(ParcelStatus.Exception);
                dbContext.TrackingEvents.Add(new TrackingEvent
                {
                    ParcelId = parcel.Id,
                    Timestamp = DateTimeOffset.UtcNow,
                    EventType = EventType.Exception,
                    Description = $"Parcel missing from manifest '{manifest.Name}'",
                    Operator = userId
                });
            }

            item.MarkMissing();
        }

        manifest.Complete();
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResult(manifest);
    }

    private static ManifestResult MapToResult(Manifest manifest)
    {
        return new ManifestResult(
            manifest.Id,
            manifest.Name,
            manifest.Status,
            manifest.DepotId,
            manifest.StartedAt,
            manifest.CompletedAt,
            manifest.Items.Count(i => i.Status == ManifestItemStatus.Expected),
            manifest.Items.Count(i => i.Status == ManifestItemStatus.Received),
            manifest.Items.Count(i => i.Status == ManifestItemStatus.Unexpected),
            manifest.Items.Count(i => i.Status == ManifestItemStatus.Missing),
            manifest.Items.Select(i => new ManifestItemResult(
                i.Id, i.TrackingNumber, i.Status, i.ParcelId)).ToList()
        );
    }
}
