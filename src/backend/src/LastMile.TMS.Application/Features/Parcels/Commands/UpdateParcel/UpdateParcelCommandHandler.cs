using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Parcels.Commands.UpdateParcel;

public class UpdateParcelCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateParcelCommand, UpdateParcelResult>
{
    private static readonly ParcelStatus[] EditableStatuses = new[]
    {
        ParcelStatus.Registered,
        ParcelStatus.ReceivedAtDepot,
        ParcelStatus.Sorted,
        ParcelStatus.Staged
    };

    public async Task<UpdateParcelResult> Handle(UpdateParcelCommand request, CancellationToken cancellationToken)
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

        if (request.ParcelType.HasValue && parcel.ParcelType != request.ParcelType.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "ParcelType", parcel.ParcelType?.ToString() ?? string.Empty, request.ParcelType.Value.ToString(), userId));
            parcel.ParcelType = request.ParcelType.Value;
        }

        // Handle ShipperAddress update - update in place
        AddressResult? shipperAddressResult = null;
        if (request.ShipperAddress != null && parcel.ShipperAddress is not null)
        {
            var address = parcel.ShipperAddress;
            var oldSummary = $"{address.Street1}, {address.City}";

            address.Street1 = request.ShipperAddress.Street1;
            address.Street2 = request.ShipperAddress.Street2;
            address.City = request.ShipperAddress.City;
            address.State = request.ShipperAddress.State;
            address.PostalCode = request.ShipperAddress.PostalCode;
            address.CountryCode = request.ShipperAddress.CountryCode;
            address.IsResidential = request.ShipperAddress.IsResidential;
            address.ContactName = request.ShipperAddress.ContactName;
            address.CompanyName = request.ShipperAddress.CompanyName;
            address.Phone = request.ShipperAddress.Phone;
            address.Email = request.ShipperAddress.Email;

            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id,
                "ShipperAddress",
                oldSummary,
                $"{address.Street1}, {address.City}",
                userId));

            shipperAddressResult = new AddressResult(
                address.Id,
                address.Street1,
                address.Street2,
                address.City,
                address.State,
                address.PostalCode,
                address.CountryCode,
                address.ContactName,
                address.CompanyName,
                address.Phone,
                address.Email);
        }

        // Handle RecipientAddress update - update in place
        AddressResult? recipientAddressResult = null;
        if (request.RecipientAddress != null && parcel.RecipientAddress is not null)
        {
            var address = parcel.RecipientAddress;
            var oldSummary = $"{address.Street1}, {address.City}";

            address.Street1 = request.RecipientAddress.Street1;
            address.Street2 = request.RecipientAddress.Street2;
            address.City = request.RecipientAddress.City;
            address.State = request.RecipientAddress.State;
            address.PostalCode = request.RecipientAddress.PostalCode;
            address.CountryCode = request.RecipientAddress.CountryCode;
            address.IsResidential = request.RecipientAddress.IsResidential;
            address.ContactName = request.RecipientAddress.ContactName;
            address.CompanyName = request.RecipientAddress.CompanyName;
            address.Phone = request.RecipientAddress.Phone;
            address.Email = request.RecipientAddress.Email;

            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id,
                "RecipientAddress",
                oldSummary,
                $"{address.Street1}, {address.City}",
                userId));

            recipientAddressResult = new AddressResult(
                address.Id,
                address.Street1,
                address.Street2,
                address.City,
                address.State,
                address.PostalCode,
                address.CountryCode,
                address.ContactName,
                address.CompanyName,
                address.Phone,
                address.Email);
        }

        if (auditLogs.Count > 0)
        {
            dbContext.ParcelAuditLogs.AddRange(auditLogs);
        }

        // Ensure parcel is marked as modified so audit fields get updated
        if (auditLogs.Count > 0 && !parcel.LastModifiedAt.HasValue)
        {
            parcel.LastModifiedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateParcelResult(
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
