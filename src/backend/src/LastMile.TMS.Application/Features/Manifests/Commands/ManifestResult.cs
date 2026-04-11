using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Manifests.Commands;

public record ManifestResult(
    Guid Id,
    string Name,
    ManifestStatus Status,
    Guid DepotId,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    int ExpectedCount,
    int ReceivedCount,
    int UnexpectedCount,
    int MissingCount,
    List<ManifestItemResult> Items
);

public record ManifestItemResult(
    Guid Id,
    string TrackingNumber,
    ManifestItemStatus Status,
    Guid? ParcelId
);
