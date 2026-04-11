using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Manifests.Commands.CreateManifest;

public class CreateManifestCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<CreateManifestCommand, ManifestResult>
{
    public async Task<ManifestResult> Handle(CreateManifestCommand request, CancellationToken cancellationToken)
    {
        var depot = await dbContext.Depots.FindAsync([request.DepotId], cancellationToken)
            ?? throw new KeyNotFoundException($"Depot with ID {request.DepotId} not found.");

        var manifest = Manifest.Create(request.Name, request.DepotId);

        foreach (var trackingNumber in request.TrackingNumbers)
        {
            var item = ManifestItem.Create(manifest.Id, trackingNumber);
            manifest.Items.Add(item);
        }

        dbContext.Manifests.Add(manifest);
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
