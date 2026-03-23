using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Depot> Depots { get; }
    DbSet<Zone> Zones { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
