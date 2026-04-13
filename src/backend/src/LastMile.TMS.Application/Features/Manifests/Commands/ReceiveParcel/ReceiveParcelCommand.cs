using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Manifests.Commands.ReceiveParcel;

public record ReceiveParcelCommand(
    Guid ManifestId,
    string TrackingNumber
) : IRequest<ReceiveParcelResult>;

public record ReceiveParcelResult(
    Guid ParcelId,
    string TrackingNumber,
    ParcelStatus NewStatus,
    ManifestItemStatus? ManifestItemStatus
);
