// using LastMile.TMS.Application.Common.Interfaces;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;

// namespace LastMile.TMS.Persistence;

// public static class DependencyInjection
// {
//     public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
//     {
//         services.AddDbContextFactory<AppDbContext>(options =>
//             options.UseNpgsql(
//                 configuration.GetConnectionString("DefaultConnection"),
//                 npgsql =>
//                 {
//                     npgsql.UseNetTopologySuite();
//                     npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
//                 }));

//         services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

//         return services;
//     }
// }




using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var npgsqlConfig = (DbContextOptionsBuilder options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.UseNetTopologySuite();
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });

        // Factory for HotChocolate resolvers — creates its own scope for ICurrentUserService
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
        {
            npgsqlConfig(options);
        }, ServiceLifetime.Scoped);  // 👈 key: make the factory itself scoped, not singleton

        // Scoped AppDbContext for Identity, OpenIddict, seeder, migrations
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
