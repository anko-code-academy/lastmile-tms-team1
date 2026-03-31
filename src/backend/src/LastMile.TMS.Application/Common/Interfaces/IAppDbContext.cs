using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Address> Addresses { get; }
    DbSet<Depot> Depots { get; }
    DbSet<DayOff> DaysOff { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Parcel> Parcels { get; }
    DbSet<Role> Roles { get; }
    DbSet<Route> Routes { get; }
    DbSet<ShiftSchedule> ShiftSchedules { get; }
    DbSet<User> Users { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleJourney> VehicleJourneys { get; }
    DbSet<Zone> Zones { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
