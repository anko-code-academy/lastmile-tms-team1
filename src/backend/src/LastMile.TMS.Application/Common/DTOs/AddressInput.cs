namespace LastMile.TMS.Application.Common.DTOs;

public record AddressInput(
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string CountryCode = "US",
    bool IsResidential = false,
    string? ContactName = null,
    string? CompanyName = null,
    string? Phone = null,
    string? Email = null);
