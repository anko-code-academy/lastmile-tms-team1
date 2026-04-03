using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Parcels.Commands.CancelParcel;

public record CancelParcelCommand(
    Guid Id,
    string Reason) : IRequest<CancelParcelResult>;

public record CancelParcelResult(
    Guid Id,
    string TrackingNumber,
    ParcelStatus Status);
