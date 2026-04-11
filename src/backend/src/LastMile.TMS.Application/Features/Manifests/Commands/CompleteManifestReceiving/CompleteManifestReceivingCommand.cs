using MediatR;

namespace LastMile.TMS.Application.Features.Manifests.Commands.CompleteManifestReceiving;

public record CompleteManifestReceivingCommand(
    Guid ManifestId
) : IRequest<ManifestResult>;
