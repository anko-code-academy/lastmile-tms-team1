using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Address> Addresses { get; }
    DbSet<Aisle> Aisles { get; }
    DbSet<Bin> Bins { get; }
    DbSet<Depot> Depots { get; }
    DbSet<DayOff> DaysOff { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Parcel> Parcels { get; }
    DbSet<Role> Roles { get; }
    DbSet<Route> Routes { get; }
    DbSet<ParcelAuditLog> ParcelAuditLogs { get; }
    DbSet<RouteStop> RouteStops { get; }
    DbSet<ShiftSchedule> ShiftSchedules { get; }
    DbSet<TrackingEvent> TrackingEvents { get; }
    DbSet<User> Users { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleJourney> VehicleJourneys { get; }
    DbSet<Zone> Zones { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
