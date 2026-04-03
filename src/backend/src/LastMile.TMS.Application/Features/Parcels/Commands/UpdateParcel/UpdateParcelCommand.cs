using AddressInput = LastMile.TMS.Application.Features.Depots.Commands.CreateDepot.AddressInput;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;

public record UpdateParcelCommand(
    Guid Id,
    string? Description,
    decimal? Weight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    ServiceType? ServiceType,
    string? ParcelType,
    AddressInput? ShipperAddress,
    AddressInput? RecipientAddress) : IRequest<ParcelResult>;

public record AddressResult(
    Guid Id,
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string CountryCode,
    string? ContactName,
    string? CompanyName,
    string? Phone,
    string? Email);

public record ParcelResult(
    Guid Id,
    string TrackingNumber,
    ParcelStatus Status,
    string? Description,
    decimal Weight,
    DateTimeOffset LastModifiedAt,
    AddressResult? ShipperAddress,
    AddressResult? RecipientAddress);
