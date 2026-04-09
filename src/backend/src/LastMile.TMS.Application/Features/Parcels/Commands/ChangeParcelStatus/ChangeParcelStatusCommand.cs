using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Parcels.Commands.ChangeParcelStatus;

public record ChangeParcelStatusCommand(
    Guid Id,
    ParcelStatus NewStatus,
    string? LocationCity = null,
    string? LocationState = null,
    string? LocationCountry = null,
    string? Description = null,
    ExceptionReason? ExceptionReason = null
) : IRequest<ChangeParcelStatusResult>;

public record ChangeParcelStatusResult(
    Guid Id,
    string TrackingNumber,
    ParcelStatus Status,
    int DeliveryAttempts);
