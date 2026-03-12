namespace LastMile.TMS.Domain.Enums;

public enum EventType
{
    LabelCreated,
    PickedUp,
    ArrivedAtFacility,
    DepartedFacility,
    InTransit,
    OutForDelivery,
    Delivered,
    DeliveryAttempted,
    Exception,
    Returned,
    AddressCorrection,
    CustomsClearance,
    HeldAtFacility
}