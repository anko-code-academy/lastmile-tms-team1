using FluentValidation;

namespace LastMile.TMS.Application.Features.Manifests.Commands.ReceiveParcel;

public class ReceiveParcelCommandValidator : AbstractValidator<ReceiveParcelCommand>
{
    public ReceiveParcelCommandValidator()
    {
        RuleFor(x => x.ManifestId).NotEmpty();
        RuleFor(x => x.TrackingNumber).NotEmpty();
    }
}
