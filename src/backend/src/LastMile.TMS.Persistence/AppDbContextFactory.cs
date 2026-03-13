using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LastMile.TMS.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = args.Length > 0 ? args[0] : "Host=localhost;Port=5432;Database=lastmile_tms;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql =>
            {
                npgsql.UseNetTopologySuite();
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });

        return new AppDbContext(optionsBuilder.Options);
    }
}