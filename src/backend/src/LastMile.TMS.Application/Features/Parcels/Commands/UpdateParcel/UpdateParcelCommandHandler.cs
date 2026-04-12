using LastMile.TMS.Application.Common.DTOs;
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
        var userName = currentUserService.UserName ?? currentUserService.UserId
            ?? throw new InvalidOperationException("User not authenticated");

        if (request.Description != null && parcel.Description != request.Description)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Description", parcel.Description ?? string.Empty, request.Description ?? string.Empty, userName));
            parcel.Description = request.Description;
        }

        if (request.Weight.HasValue && parcel.Weight != request.Weight.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Weight", parcel.Weight.ToString(), request.Weight.Value.ToString(), userName));
            parcel.Weight = request.Weight.Value;
        }

        if (request.Length.HasValue && parcel.Length != request.Length.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Length", parcel.Length.ToString(), request.Length.Value.ToString(), userName));
            parcel.Length = request.Length.Value;
        }

        if (request.Width.HasValue && parcel.Width != request.Width.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Width", parcel.Width.ToString(), request.Width.Value.ToString(), userName));
            parcel.Width = request.Width.Value;
        }

        if (request.Height.HasValue && parcel.Height != request.Height.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "Height", parcel.Height.ToString(), request.Height.Value.ToString(), userName));
            parcel.Height = request.Height.Value;
        }

        if (request.ServiceType.HasValue && parcel.ServiceType != request.ServiceType.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "ServiceType", parcel.ServiceType.ToString(), request.ServiceType.Value.ToString(), userName));
            parcel.ServiceType = request.ServiceType.Value;
        }

        if (request.ParcelType.HasValue && parcel.ParcelType != request.ParcelType.Value)
        {
            auditLogs.Add(ParcelAuditLog.Create(
                parcel.Id, "ParcelType", parcel.ParcelType?.ToString() ?? string.Empty, request.ParcelType.Value.ToString(), userName));
            parcel.ParcelType = request.ParcelType.Value;
        }

        // Handle ShipperAddress update — log per-field changes, then apply
        AddressResult? shipperAddressResult = null;
        if (request.ShipperAddress != null && parcel.ShipperAddress is not null)
        {
            LogAddressChanges(auditLogs, parcel.Id, "ShipperAddress",
                parcel.ShipperAddress, request.ShipperAddress, userName);
            ApplyAddressUpdate(parcel.ShipperAddress, request.ShipperAddress);

            shipperAddressResult = new AddressResult(
                parcel.ShipperAddress.Id,
                parcel.ShipperAddress.Street1,
                parcel.ShipperAddress.Street2,
                parcel.ShipperAddress.City,
                parcel.ShipperAddress.State,
                parcel.ShipperAddress.PostalCode,
                parcel.ShipperAddress.CountryCode,
                parcel.ShipperAddress.ContactName,
                parcel.ShipperAddress.CompanyName,
                parcel.ShipperAddress.Phone,
                parcel.ShipperAddress.Email);
        }

        // Handle RecipientAddress update — log per-field changes, then apply
        AddressResult? recipientAddressResult = null;
        if (request.RecipientAddress != null && parcel.RecipientAddress is not null)
        {
            LogAddressChanges(auditLogs, parcel.Id, "RecipientAddress",
                parcel.RecipientAddress, request.RecipientAddress, userName);
            ApplyAddressUpdate(parcel.RecipientAddress, request.RecipientAddress);

            recipientAddressResult = new AddressResult(
                parcel.RecipientAddress.Id,
                parcel.RecipientAddress.Street1,
                parcel.RecipientAddress.Street2,
                parcel.RecipientAddress.City,
                parcel.RecipientAddress.State,
                parcel.RecipientAddress.PostalCode,
                parcel.RecipientAddress.CountryCode,
                parcel.RecipientAddress.ContactName,
                parcel.RecipientAddress.CompanyName,
                parcel.RecipientAddress.Phone,
                parcel.RecipientAddress.Email);
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
            parcel.LastModifiedAt ?? parcel.CreatedAt,
            shipperAddressResult,
            recipientAddressResult);
    }

    private static void LogAddressChanges(List<ParcelAuditLog> logs, Guid parcelId,
        string prefix, Address existing, AddressInput updated, string userName)
    {
        CompareField("Street1", existing.Street1, updated.Street1);
        CompareField("Street2", existing.Street2, updated.Street2);
        CompareField("City", existing.City, updated.City);
        CompareField("State", existing.State, updated.State);
        CompareField("PostalCode", existing.PostalCode, updated.PostalCode);
        CompareField("CountryCode", existing.CountryCode, updated.CountryCode);
        CompareField("IsResidential", existing.IsResidential.ToString(), updated.IsResidential.ToString());
        CompareField("ContactName", existing.ContactName, updated.ContactName);
        CompareField("CompanyName", existing.CompanyName, updated.CompanyName);
        CompareField("Phone", existing.Phone, updated.Phone);
        CompareField("Email", existing.Email, updated.Email);

        void CompareField(string field, string? oldVal, string? newVal)
        {
            if (oldVal != newVal)
                logs.Add(ParcelAuditLog.Create(parcelId, $"{prefix}.{field}", oldVal ?? "", newVal ?? "", userName));
        }
    }

    private static void ApplyAddressUpdate(Address existing, AddressInput updated)
    {
        existing.Street1 = updated.Street1;
        existing.Street2 = updated.Street2;
        existing.City = updated.City;
        existing.State = updated.State;
        existing.PostalCode = updated.PostalCode;
        existing.CountryCode = updated.CountryCode;
        existing.IsResidential = updated.IsResidential;
        existing.ContactName = updated.ContactName;
        existing.CompanyName = updated.CompanyName;
        existing.Phone = updated.Phone;
        existing.Email = updated.Email;
    }
}
