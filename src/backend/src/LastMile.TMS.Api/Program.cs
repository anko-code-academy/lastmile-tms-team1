using Hangfire;
using Hangfire.PostgreSql;
using LastMile.TMS.Application;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Infrastructure;
using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using DbSeeder = LastMile.TMS.Api.Services.DbSeeder;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddPersistence(builder.Configuration);

    // Configure Identity
    builder.Services.AddIdentity<User, Role>(options =>
    {
        // Password requirements
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // Configure OpenIddict server (must be after AddIdentity and AddPersistence)
    var accessTokenMinutes = builder.Configuration.GetValue("Authentication:AccessTokenLifetimeMinutes", 15);
    var refreshTokenDays = builder.Configuration.GetValue("Authentication:RefreshTokenLifetimeDays", 7);

    builder.Services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
        })
        .AddServer(options =>
        {
            // Enable password and refresh-token grant types
            options.AllowPasswordFlow()
                   .AllowRefreshTokenFlow();

            // Token endpoint
            options.SetTokenEndpointUris("/connect/token");

            // Accept anonymous clients (no client_id required for password flow)
            options.AcceptAnonymousClients();

            // Register scopes
            options.RegisterScopes(OpenIddictConstants.Scopes.OfflineAccess);

            // Token lifetimes
            options.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenMinutes));
            options.SetRefreshTokenLifetime(TimeSpan.FromDays(refreshTokenDays));

            // Use ASP.NET Core integration with passthrough to controller
            options.UseAspNetCore()
                   .EnableTokenEndpointPassthrough()
                   .DisableTransportSecurityRequirement();

            // Ephemeral signing/encryption keys (dev-only; swap for real certs in prod)
            options.AddEphemeralEncryptionKey()
                   .AddEphemeralSigningKey()
                   .DisableAccessTokenEncryption(); // plain JWT (not encrypted JWE)
        })
        .AddValidation(options =>
        {
            // Validate tokens issued by the local OpenIddict server
            options.UseLocalServer();
            options.UseAspNetCore();
        });

    // Override default auth scheme set by AddIdentity (cookie) → use OpenIddict JWT validation.
    // Must be called AFTER AddPersistence (which registers Identity) and AddInfrastructure (OpenIddict).
    builder.Services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
    {
        options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();

    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = builder.Configuration.GetConnectionString("Redis"));

    builder.Services.AddHangfire(config =>
        config.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"))));
    builder.Services.AddHangfireServer();

    // Register DbSeeder
    builder.Services.AddScoped<LastMile.TMS.Application.Common.Interfaces.IDbSeeder, DbSeeder>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHangfireDashboard("/hangfire");

    // Apply migrations then seed database
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    var seeder = scope.ServiceProvider.GetRequiredService<LastMile.TMS.Application.Common.Interfaces.IDbSeeder>();
    await seeder.SeedAsync();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory in integration tests
namespace LastMile.TMS.Api
{
    public partial class Program;
}