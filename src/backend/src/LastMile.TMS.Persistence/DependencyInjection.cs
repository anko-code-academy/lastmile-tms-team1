using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        Action<DbContextOptionsBuilder> npgsqlOptions = options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.UseNetTopologySuite();
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });

        // Primary registration — used by GraphQL, MediatR, Identity, OpenIddict
        services.AddDbContext<AppDbContext>((sp, options) => npgsqlOptions(options));

        // Factory — only for Hangfire background jobs
        services.AddDbContextFactory<AppDbContext>((sp, options) => npgsqlOptions(options),
            ServiceLifetime.Scoped);

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
