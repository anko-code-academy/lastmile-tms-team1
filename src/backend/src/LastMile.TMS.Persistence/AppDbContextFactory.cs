using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LastMile.TMS.Persistence;

/// <summary>
/// Required for EF Core design-time tools (migrations).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Priority: 1) command-line args, 2) environment variable, 3) throw error
        var connectionString = args.Length > 0
            ? args[0]
            : Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                ?? throw new InvalidOperationException(
                    "Connection string not provided. Pass as argument or set DEFAULT_CONNECTION environment variable.");

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