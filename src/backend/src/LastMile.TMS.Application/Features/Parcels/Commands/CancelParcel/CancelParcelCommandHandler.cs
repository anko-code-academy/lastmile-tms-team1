using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;

public class CancelParcelCommandHandler(
    IAppDbContext dbContext) : IRequestHandler<CancelParcelCommand, CancelParcelResult>
{
    public async Task<CancelParcelResult> Handle(CancelParcelCommand request, CancellationToken cancellationToken)
    {
        var parcel = await dbContext.Parcels
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Parcel with ID {request.Id} not found.");

        // Use domain logic for status transition validation
        if (!parcel.CanTransitionTo(Domain.Enums.ParcelStatus.Cancelled))
        {
            throw new InvalidOperationException(
                $"Cannot cancel parcel in status {parcel.Status}. " +
                "Parcel can only be cancelled before being loaded for delivery.");
        }

        // Transition to Cancelled
        parcel.TransitionTo(Domain.Enums.ParcelStatus.Cancelled);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CancelParcelResult(
            parcel.Id,
            parcel.TrackingNumber,
            parcel.Status);
    }
}
