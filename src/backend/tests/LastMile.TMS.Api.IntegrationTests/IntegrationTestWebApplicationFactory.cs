using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetTopologySuite;

namespace LastMile.TMS.Api.IntegrationTests;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _postgreSqlFixture;

    public IntegrationTestWebApplicationFactory(PostgreSqlContainerFixture postgreSqlFixture)
    {
        _postgreSqlFixture = postgreSqlFixture;
    }

    public string GetConnectionString() => _postgreSqlFixture.ConnectionString;

    public async Task InitializeAsync()
    {
        await _postgreSqlFixture.InitializeAsync();

        // Apply migrations using the test database
        var connectionString = _postgreSqlFixture.ConnectionString;
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, o => o.UseNetTopologySuite())
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

        var currentUserService = new StubCurrentUserService();
        await using var dbContext = new AppDbContext(optionsBuilder.Options, currentUserService);
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
            // Register stub FIRST as singleton so it's available at root scope
            // (replaces any scoped ICurrentUserService from Infrastructure)
            var currentUserDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(LastMile.TMS.Application.Common.Interfaces.ICurrentUserService));
            if (currentUserDescriptor != null)
                services.Remove(currentUserDescriptor);

            services.AddSingleton<LastMile.TMS.Application.Common.Interfaces.ICurrentUserService, StubCurrentUserService>();

            // Remove all EF Core / DbContext registrations
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(IDbContextFactory<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType == typeof(LastMile.TMS.Application.Common.Interfaces.IAppDbContext) ||
                // Remove EF Core options configurations (the root cause of the scoped error)
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                 d.ServiceType.GenericTypeArguments[0].IsGenericType &&
                 d.ServiceType.GenericTypeArguments[0].GetGenericTypeDefinition()
                     .Name.StartsWith("IDbContextOptionsConfiguration"))
            ).ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            // Remove hosted services (Redis, Hangfire) that hang in tests
            var hostedServices = services
                .Where(d => d.ServiceType.IsAssignableTo(typeof(IHostedService)))
                .ToList();
            foreach (var svc in hostedServices)
                services.Remove(svc);

            // Re-register cleanly with test connection string
            services.AddDbContextFactory<AppDbContext>((sp, options) =>
                options.UseNpgsql(
                    _postgreSqlFixture.ConnectionString,
                    o => o.UseNetTopologySuite())
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)),
                ServiceLifetime.Scoped);

            services.AddScoped<AppDbContext>(sp =>
                sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            services.AddScoped<LastMile.TMS.Application.Common.Interfaces.IAppDbContext>(
                sp => sp.GetRequiredService<AppDbContext>());
        });

        builder.UseSetting("AdminCredentials:Username",
            Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin");
        builder.UseSetting("AdminCredentials:Email",
            Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@lastmile.com");
        builder.UseSetting("AdminCredentials:Password",
            Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123");
    }
}
