using FluentValidation;

namespace LastMile.TMS.Application.Features.Manifests.Commands.CompleteManifestReceiving;

public class CompleteManifestReceivingCommandValidator : AbstractValidator<CompleteManifestReceivingCommand>
{
    public CompleteManifestReceivingCommandValidator()
    {
        RuleFor(x => x.ManifestId).NotEmpty();
    }
}
