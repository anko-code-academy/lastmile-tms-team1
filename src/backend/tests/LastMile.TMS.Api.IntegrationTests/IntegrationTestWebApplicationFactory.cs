using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using LastMile.TMS.Persistence;

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
            .UseNpgsql(connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

        await using var dbContext = new AppDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlFixture.DisposeAsync();
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
                    .UseNpgsql(_postgreSqlFixture.ConnectionString)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
        });
    }
}
