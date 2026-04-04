using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

public record ParcelResult(
    Guid Id,
    string TrackingNumber,
    ParcelStatus Status,
    ServiceType ServiceType,
    DateTimeOffset CreatedAt);
