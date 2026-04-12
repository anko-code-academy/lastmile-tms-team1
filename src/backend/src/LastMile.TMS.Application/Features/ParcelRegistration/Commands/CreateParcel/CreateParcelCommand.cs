using LastMile.TMS.Application.Common.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

public record CreateParcelCommand(
    string? Description,
    ServiceType ServiceType,
    AddressInput ShipperAddress,
    AddressInput RecipientAddress,
    decimal Weight,
    WeightUnit WeightUnit,
    decimal Length,
    decimal Width,
    decimal Height,
    DimensionUnit DimensionUnit,
    decimal DeclaredValue,
    string Currency = "USD",
    ParcelType? ParcelType = null,
    string? Notes = null
) : IRequest<CreateParcelResult>;
