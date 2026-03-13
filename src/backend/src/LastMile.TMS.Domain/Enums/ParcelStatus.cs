namespace LastMile.TMS.Domain.Enums;

public enum ParcelStatus
{
    Registered,
    ReceivedAtDepot,
    Sorted,
    Staged,
    Loaded,
    OutForDelivery,
    Delivered,
    FailedAttempt,
    ReturnedToDepot,
    Cancelled,
    Exception
}