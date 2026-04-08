using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

public class CreateParcelHandler(
    IAppDbContext dbContext,
    IGeocodingService geocodingService,
    Domain.Services.IDeliveryDateCalculator deliveryDateCalculator)
    : IRequestHandler<CreateParcelCommand, ParcelResult>
{
    public async Task<ParcelResult> Handle(CreateParcelCommand request, CancellationToken cancellationToken)
    {
        // 1. Build full address strings for geocoding
        var shipperFullAddress = BuildFullAddress(request.ShipperAddress);
        var recipientFullAddress = BuildFullAddress(request.RecipientAddress);

        // 2. Geocode addresses
        var shipperGeoPoint = await geocodingService.GeocodeAsync(shipperFullAddress, cancellationToken);
        var recipientGeoPoint = await geocodingService.GeocodeAsync(recipientFullAddress, cancellationToken);

        if (recipientGeoPoint == null)
        {
            throw new InvalidOperationException("Recipient address could not be geocoded. Please provide a valid address.");
        }

        if (shipperGeoPoint == null)
        {
            throw new InvalidOperationException("Shipper address could not be geocoded. Please provide a valid address.");
        }

        // 3. Create address entities
        var shipperAddress = new Address
        {
            Street1 = request.ShipperAddress.Street1,
            Street2 = request.ShipperAddress.Street2,
            City = request.ShipperAddress.City,
            State = request.ShipperAddress.State,
            PostalCode = request.ShipperAddress.PostalCode,
            CountryCode = request.ShipperAddress.CountryCode,
            IsResidential = request.ShipperAddress.IsResidential,
            ContactName = request.ShipperAddress.ContactName,
            CompanyName = request.ShipperAddress.CompanyName,
            Phone = request.ShipperAddress.Phone,
            Email = request.ShipperAddress.Email,
            GeoLocation = shipperGeoPoint
        };

        var recipientAddress = new Address
        {
            Street1 = request.RecipientAddress.Street1,
            Street2 = request.RecipientAddress.Street2,
            City = request.RecipientAddress.City,
            State = request.RecipientAddress.State,
            PostalCode = request.RecipientAddress.PostalCode,
            CountryCode = request.RecipientAddress.CountryCode,
            IsResidential = request.RecipientAddress.IsResidential,
            ContactName = request.RecipientAddress.ContactName,
            CompanyName = request.RecipientAddress.CompanyName,
            Phone = request.RecipientAddress.Phone,
            Email = request.RecipientAddress.Email,
            GeoLocation = recipientGeoPoint
        };

        // 4. Create parcel using factory (generates tracking number, sets status to Registered)
        var parcel = Parcel.Create(request.Description, request.ServiceType);

        // 5. Assign physical properties
        parcel.Weight = request.Weight;
        parcel.WeightUnit = request.WeightUnit;
        parcel.Length = request.Length;
        parcel.Width = request.Width;
        parcel.Height = request.Height;
        parcel.DimensionUnit = request.DimensionUnit;
        parcel.DeclaredValue = request.DeclaredValue;
        parcel.Currency = request.Currency;
        parcel.ParcelType = request.ParcelType;
        parcel.Notes = request.Notes;
        parcel.EstimatedDeliveryDate = deliveryDateCalculator.CalculateEstimatedDeliveryDate(
            request.ServiceType, DateTimeOffset.UtcNow);

        // 6. Assign addresses (EF Core will set IDs after SaveChangesAsync)
        parcel.ShipperAddress = shipperAddress;
        parcel.RecipientAddress = recipientAddress;

        // 7. Auto-assign zone based on recipient geolocation
        if (recipientGeoPoint != null)
        {
            var zone = await FindZoneContainingPointAsync(recipientGeoPoint, cancellationToken);
            parcel.ZoneId = zone?.Id;
            parcel.Zone = zone;
        }

        // 8. Save to database
        dbContext.Addresses.Add(shipperAddress);
        dbContext.Addresses.Add(recipientAddress);
        dbContext.Parcels.Add(parcel);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 9. Return result
        return new ParcelResult(
            parcel.Id,
            parcel.TrackingNumber,
            parcel.Status,
            parcel.ServiceType,
            parcel.CreatedAt,
            parcel.EstimatedDeliveryDate!.Value);
    }

    private static string BuildFullAddress(ParcelAddressInput address)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(address.Street1))
            parts.Add(address.Street1);
        if (!string.IsNullOrWhiteSpace(address.Street2))
            parts.Add(address.Street2);
        if (!string.IsNullOrWhiteSpace(address.City))
            parts.Add(address.City);
        if (!string.IsNullOrWhiteSpace(address.State))
            parts.Add(address.State);
        if (!string.IsNullOrWhiteSpace(address.PostalCode))
            parts.Add(address.PostalCode);
        if (!string.IsNullOrWhiteSpace(address.CountryCode))
            parts.Add(address.CountryCode);

        return string.Join(", ", parts);
    }

    private async Task<Zone?> FindZoneContainingPointAsync(Point point, CancellationToken cancellationToken)
    {
        return await dbContext.Zones
            .AsNoTracking()
            .Where(z => z.IsActive && z.BoundaryGeometry != null && z.BoundaryGeometry.Contains(point))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
