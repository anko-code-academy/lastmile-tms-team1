using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Parcels.Commands.ScanParcel;

public record ScanParcelCommand(
    string TrackingNumber,
    ParcelStatus NewStatus,
    Guid? RouteId = null,
    string? Description = null,
    Guid? ManifestId = null
) : IRequest<ScanParcelResult>;

public record ScanParcelResult(
    Guid Id,
    string TrackingNumber,
    ParcelStatus PreviousStatus,
    ParcelStatus NewStatus,
    string? ZoneName,
    string? BinLabel,
    string? RouteName
);
