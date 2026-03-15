using Hangfire;
using Hangfire.PostgreSql;
using LastMile.TMS.Application;
using LastMile.TMS.Infrastructure;
using LastMile.TMS.Infrastructure.Options;
using LastMile.TMS.Infrastructure.Seeding;
using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using Serilog;

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

    // Add ASP.NET Core Identity
    builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // Configure OpenIddict
    var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
    builder.Services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                .UseDbContext<AppDbContext>();
        })
        .AddServer(options =>
        {
            // Enable passthrough - AuthController handles token generation
            options.UseAspNetCore()
                .EnableTokenEndpointPassthrough();

            var accessTokenLifetime = TimeSpan.Parse(authOptions.AccessTokenLifetime);
            var refreshTokenLifetime = TimeSpan.Parse(authOptions.RefreshTokenLifetime);

            options.SetAccessTokenLifetime(accessTokenLifetime);
            options.SetRefreshTokenLifetime(refreshTokenLifetime);

            options.RegisterScopes("api", "offline_access");

            options.AddDevelopmentEncryptionCertificate();
            options.AddDevelopmentSigningCertificate();

            options.RegisterAudiences(authOptions.Audience);

            options.SetTokenEndpointUris("/connect/token");

            options.AllowPasswordFlow();
            options.AllowRefreshTokenFlow();

            // Accept client_id-less requests for password flow
            options.AcceptAnonymousClients();
        })
        .AddValidation(options =>
        {
            options.UseLocalServer();
        });

    var app = builder.Build();

    // Seed identity data
    using (var scope = app.Services.CreateScope())
    {
        await IdentitySeedData.SeedAsync(scope.ServiceProvider);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    // Allow HTTP for development (OpenIddict normally requires HTTPS)
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHangfireDashboard("/hangfire");

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