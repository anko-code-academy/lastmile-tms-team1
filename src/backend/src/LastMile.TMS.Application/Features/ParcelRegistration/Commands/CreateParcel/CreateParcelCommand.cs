using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

public record ParcelAddressInput(
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string CountryCode = "US",
    bool IsResidential = false,
    string? ContactName = null,
    string? CompanyName = null,
    string? Phone = null,
    string? Email = null);

public record CreateParcelCommand(
    string? Description,
    ServiceType ServiceType,
    ParcelAddressInput ShipperAddress,
    ParcelAddressInput RecipientAddress,
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
) : IRequest<ParcelResult>;
