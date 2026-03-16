using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Parcel : BaseAuditableEntity
{
    public string TrackingNumber { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public ServiceType ServiceType { get; set; }
    public ParcelStatus Status { get; set; }

    // Addresses
    public Guid ShipperAddressId { get; set; }
    public Address ShipperAddress { get; set; } = null!;

    public Guid RecipientAddressId { get; set; }
    public Address RecipientAddress { get; set; } = null!;

    // Weight & Dimensions
    public decimal Weight { get; set; }
    public WeightUnit WeightUnit { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public DimensionUnit DimensionUnit { get; set; }

    // Value
    public decimal DeclaredValue { get; set; }
    public string Currency { get; set; } = "USD";

    // Dates
    public DateTimeOffset? EstimatedDeliveryDate { get; set; }
    public DateTimeOffset? ActualDeliveryDate { get; set; }

    // Delivery
    public int DeliveryAttempts { get; set; }
    public string? ParcelType { get; set; }

    // Zone (auto-assigned via geocoding)
    public Guid? ZoneId { get; set; }
    // public Zone? Zone { get; set; }

    // Navigation properties
    public DeliveryConfirmation? DeliveryConfirmation { get; set; }
    public ICollection<ParcelContentItem> ContentItems { get; set; } = new List<ParcelContentItem>();
    public ICollection<TrackingEvent> TrackingEvents { get; set; } = new List<TrackingEvent>();
    public ICollection<ParcelWatcher> ParcelWatchers { get; set; } = new List<ParcelWatcher>();

    // Factory method for tracking number generation
    public static string GenerateTrackingNumber()
    {
        // Format: LMTT1-YYYYMMDD-XXXXXX (16 chars)
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"LMTT1-{datePart}-{randomPart}";
    }

    // Domain logic: Initialize tracking number
    public static Parcel Create(string? description, ServiceType serviceType)
    {
        return new Parcel
        {
            TrackingNumber = GenerateTrackingNumber(),
            Description = description,
            ServiceType = serviceType,
            Status = ParcelStatus.Registered,
            DeliveryAttempts = 0
        };
    }

    // Status transition validation
    public bool CanTransitionTo(ParcelStatus newStatus)
    {
        return newStatus switch
        {
            ParcelStatus.Registered => false, // Cannot go back to Registered

            ParcelStatus.ReceivedAtDepot => Status == ParcelStatus.Registered,
            ParcelStatus.Sorted => Status == ParcelStatus.ReceivedAtDepot,
            ParcelStatus.Staged => Status == ParcelStatus.Sorted,
            ParcelStatus.Loaded => Status == ParcelStatus.Staged,
            ParcelStatus.OutForDelivery => Status == ParcelStatus.Loaded || Status == ParcelStatus.FailedAttempt,
            ParcelStatus.Delivered => Status == ParcelStatus.OutForDelivery,
            ParcelStatus.FailedAttempt => Status == ParcelStatus.OutForDelivery,
            ParcelStatus.ReturnedToDepot => Status == ParcelStatus.FailedAttempt || Status == ParcelStatus.Exception,
            ParcelStatus.Cancelled => Status == ParcelStatus.Registered || Status == ParcelStatus.ReceivedAtDepot,
            ParcelStatus.Exception => Status != ParcelStatus.Delivered, // Can exception from any non-delivered status

            _ => false
        };
    }

    public void TransitionTo(ParcelStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
        }

        Status = newStatus;

        if (newStatus == ParcelStatus.Delivered)
        {
            ActualDeliveryDate = DateTimeOffset.UtcNow;
        }
    }
}