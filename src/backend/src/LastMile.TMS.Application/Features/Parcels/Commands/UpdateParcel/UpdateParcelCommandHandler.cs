using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;

public class UpdateParcelCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateParcelCommand, ParcelResult>
{
    private static readonly ParcelStatus[] EditableStatuses = new[]
    {
        ParcelStatus.Registered,
        ParcelStatus.ReceivedAtDepot,
        ParcelStatus.Sorted,
        ParcelStatus.Staged
    };

    public async Task<ParcelResult> Handle(UpdateParcelCommand request, CancellationToken cancellationToken)
    {
        var parcel = await dbContext.Parcels
            .Include(p => p.ShipperAddress)
            .Include(p => p.RecipientAddress)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Parcel with ID {request.Id} not found.");

        // Validate status
        if (!EditableStatuses.Contains(parcel.Status))
        {
            throw new InvalidOperationException(
                $"Cannot edit parcel in status {parcel.Status}. " +
                $"Allowed statuses: {string.Join(", ", EditableStatuses.Select(s => s.ToString()))}");
        }

        // Track changes for audit
        var auditLogs = new List<ParcelAuditLog>();
        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated");

        if (request.Description != null && parcel.Description != request.Description)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Description", parcel.Description ?? string.Empty, request.Description ?? string.Empty, userId));
            parcel.Description = request.Description;
        }

        if (request.Weight.HasValue && parcel.Weight != request.Weight.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Weight", parcel.Weight.ToString(), request.Weight.Value.ToString(), userId));
            parcel.Weight = request.Weight.Value;
        }

        if (request.Length.HasValue && parcel.Length != request.Length.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Length", parcel.Length.ToString(), request.Length.Value.ToString(), userId));
            parcel.Length = request.Length.Value;
        }

        if (request.Width.HasValue && parcel.Width != request.Width.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Width", parcel.Width.ToString(), request.Width.Value.ToString(), userId));
            parcel.Width = request.Width.Value;
        }

        if (request.Height.HasValue && parcel.Height != request.Height.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Height", parcel.Height.ToString(), request.Height.Value.ToString(), userId));
            parcel.Height = request.Height.Value;
        }

        if (request.ServiceType.HasValue && parcel.ServiceType != request.ServiceType.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "ServiceType", parcel.ServiceType.ToString(), request.ServiceType.Value.ToString(), userId));
            parcel.ServiceType = request.ServiceType.Value;
        }

        if (request.ParcelType != null && parcel.ParcelType != request.ParcelType)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "ParcelType", parcel.ParcelType ?? string.Empty, request.ParcelType ?? string.Empty, userId));
            parcel.ParcelType = request.ParcelType;
        }

        // Handle ShipperAddress update - create new address
        AddressResult? shipperAddressResult = null;
        if (request.ShipperAddress != null)
        {
            var oldAddress = parcel.ShipperAddress;
            var newAddress = new Address
            {
                Id = Guid.NewGuid(),
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
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            dbContext.Addresses.Add(newAddress);
            parcel.ShipperAddressId = newAddress.Id;

            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id,
                "ShipperAddress",
                oldAddress != null ? $"{oldAddress.Street1}, {oldAddress.City}" : "N/A",
                $"{newAddress.Street1}, {newAddress.City}",
                userId));

            shipperAddressResult = new AddressResult(
                newAddress.Id,
                newAddress.Street1,
                newAddress.Street2,
                newAddress.City,
                newAddress.State,
                newAddress.PostalCode,
                newAddress.CountryCode,
                newAddress.ContactName,
                newAddress.CompanyName,
                newAddress.Phone,
                newAddress.Email);
        }

        // Handle RecipientAddress update - create new address
        AddressResult? recipientAddressResult = null;
        if (request.RecipientAddress != null)
        {
            var oldAddress = parcel.RecipientAddress;
            var newAddress = new Address
            {
                Id = Guid.NewGuid(),
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
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            dbContext.Addresses.Add(newAddress);
            parcel.RecipientAddressId = newAddress.Id;

            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id,
                "RecipientAddress",
                oldAddress != null ? $"{oldAddress.Street1}, {oldAddress.City}" : "N/A",
                $"{newAddress.Street1}, {newAddress.City}",
                userId));

            recipientAddressResult = new AddressResult(
                newAddress.Id,
                newAddress.Street1,
                newAddress.Street2,
                newAddress.City,
                newAddress.State,
                newAddress.PostalCode,
                newAddress.CountryCode,
                newAddress.ContactName,
                newAddress.CompanyName,
                newAddress.Phone,
                newAddress.Email);
        }

        if (auditLogs.Count > 0)
        {
            dbContext.ParcelAuditLogs.AddRange(auditLogs);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ParcelResult(
            parcel.Id,
            parcel.TrackingNumber,
            parcel.Status,
            parcel.Description,
            parcel.Weight,
            parcel.LastModifiedAt!.Value,
            shipperAddressResult,
            recipientAddressResult);
    }
}
