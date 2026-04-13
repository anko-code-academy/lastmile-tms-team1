using FluentValidation;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.Parcels.Commands.ScanParcel;

public class ScanParcelCommandValidator : AbstractValidator<ScanParcelCommand>
{
    private static readonly HashSet<ParcelStatus> AllowedStatuses =
    [
        ParcelStatus.ReceivedAtDepot,
        ParcelStatus.Sorted,
        ParcelStatus.Staged,
        ParcelStatus.Loaded,
        ParcelStatus.ReturnedToDepot
    ];

    public ScanParcelCommandValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .Matches(@"^LM-\d{6}-[A-Z0-9]{6}$")
            .WithMessage("Tracking number must match format LM-YYMMDD-XXXXXX");

        RuleFor(x => x.NewStatus)
            .Must(status => AllowedStatuses.Contains(status))
            .WithMessage("Status must be one of: ReceivedAtDepot, Sorted, Staged, Loaded, ReturnedToDepot");
    }
}
