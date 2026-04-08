using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

public record CreateParcelResult(
    Guid Id,
    string TrackingNumber,
    ParcelStatus Status,
    ServiceType ServiceType,
    DateTimeOffset CreatedAt,
    DateTimeOffset EstimatedDeliveryDate);
