using FluentValidation;

namespace LastMile.TMS.Application.Features.Manifests.Commands.CreateManifest;

public class CreateManifestCommandValidator : AbstractValidator<CreateManifestCommand>
{
    public CreateManifestCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DepotId)
            .NotEmpty();

        RuleFor(x => x.TrackingNumbers)
            .NotEmpty()
            .WithMessage("At least one tracking number is required");
    }
}
