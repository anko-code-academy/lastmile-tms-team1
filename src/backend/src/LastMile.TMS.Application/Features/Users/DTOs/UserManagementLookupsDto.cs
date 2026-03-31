namespace LastMile.TMS.Application.Features.Users.DTOs;

public record RoleLookupDto(Guid Id, string Name, string? Description);
public record DepotLookupDto(Guid Id, string Name);
public record ZoneLookupDto(Guid Id, string Name, Guid DepotId);
public record UserManagementLookupsDto(
    IReadOnlyList<RoleLookupDto> Roles,
    IReadOnlyList<DepotLookupDto> Depots,
    IReadOnlyList<ZoneLookupDto> Zones
);
