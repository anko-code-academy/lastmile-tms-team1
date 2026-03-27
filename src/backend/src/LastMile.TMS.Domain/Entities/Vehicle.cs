using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Vehicle : BaseAuditableEntity
{
    public string RegistrationPlate { get; private set; } = string.Empty;
    public VehicleType Type { get; set; }
    public int ParcelCapacity { get; set; }
    public decimal WeightCapacityKg { get; set; }
    public VehicleStatus Status { get; set; }

    // Depot assignment
    public Guid? DepotId { get; set; }
    public Depot? Depot { get; set; }

    // Factory method
    public static Vehicle Create(
        string registrationPlate,
        VehicleType type,
        int parcelCapacity,
        decimal weightCapacityKg)
    {
        return new Vehicle
        {
            RegistrationPlate = registrationPlate.ToUpperInvariant(),
            Type = type,
            ParcelCapacity = parcelCapacity,
            WeightCapacityKg = weightCapacityKg,
            Status = VehicleStatus.Available
        };
    }

    // Status transition validation
    public bool CanTransitionTo(VehicleStatus newStatus)
    {
        return newStatus switch
        {
            VehicleStatus.Available => Status == VehicleStatus.InUse || Status == VehicleStatus.Maintenance,

            VehicleStatus.InUse => Status == VehicleStatus.Available,

            VehicleStatus.Maintenance => Status == VehicleStatus.Available || Status == VehicleStatus.InUse,

            VehicleStatus.Retired => Status == VehicleStatus.Maintenance || Status == VehicleStatus.Available,

            _ => false
        };
    }

    public void TransitionTo(VehicleStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
        }

        Status = newStatus;
    }

    public void AssignToRoute(int parcelCount)
    {
        if (Status == VehicleStatus.Retired)
        {
            throw new InvalidOperationException("Cannot assign a retired vehicle to a route");
        }

        if (parcelCount > ParcelCapacity)
        {
            throw new InvalidOperationException($"Vehicle capacity exceeded. Max: {ParcelCapacity}, Requested: {parcelCount}");
        }

        if (Status != VehicleStatus.InUse && !CanTransitionTo(VehicleStatus.InUse))
        {
             throw new InvalidOperationException($"Vehicle is not available for assignment (Current status: {Status})");
        }

        Status = VehicleStatus.InUse;
    }

    public void ReleaseFromRoute()
    {
        if (Status == VehicleStatus.InUse)
        {
            Status = VehicleStatus.Available;
        }
    }

    public void Update(
        string registrationPlate,
        VehicleType type,
        int parcelCapacity,
        decimal weightCapacityKg,
        Guid? depotId)
    {
        RegistrationPlate = registrationPlate.ToUpperInvariant();
        Type = type;
        ParcelCapacity = parcelCapacity;
        WeightCapacityKg = weightCapacityKg;
        DepotId = depotId;
    }
}
