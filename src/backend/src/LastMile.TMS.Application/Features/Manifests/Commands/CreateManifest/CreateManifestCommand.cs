using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Manifests.Commands.CreateManifest;

public record CreateManifestCommand(
    string Name,
    Guid DepotId,
    List<string> TrackingNumbers
) : IRequest<ManifestResult>;
