using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using LastMile.TMS.Persistence;
using NetTopologySuite;

namespace LastMile.TMS.Api.IntegrationTests;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _postgreSqlFixture;

    public IntegrationTestWebApplicationFactory(PostgreSqlContainerFixture postgreSqlFixture)
    {
        _postgreSqlFixture = postgreSqlFixture;
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlFixture.InitializeAsync();

        // Apply migrations using the test database
        var connectionString = _postgreSqlFixture.ConnectionString;
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, o => o.UseNetTopologySuite())
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

        await using var dbContext = new AppDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync();
    }

    public new Task DisposeAsync()
    {
        // Don't dispose the fixture here - it's managed by ICollectionFixture
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .UseNpgsql(_postgreSqlFixture.ConnectionString, o => o.UseNetTopologySuite())
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
        });

        builder.UseSetting("AdminCredentials:Username", Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin");
        builder.UseSetting("AdminCredentials:Email", Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@lastmile.com");
        builder.UseSetting("AdminCredentials:Password", Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123");
    }
}
