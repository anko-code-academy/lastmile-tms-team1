namespace LastMile.TMS.Domain.Enums;

public enum ExceptionReason
{
    AddressNotFound,
    RecipientUnavailable,
    DamagedInTransit,
    WeatherDelay,
    CustomsHold,
    RefusedByRecipient,
    BadLabel,
    Unidentified,
    CustomerHold
}