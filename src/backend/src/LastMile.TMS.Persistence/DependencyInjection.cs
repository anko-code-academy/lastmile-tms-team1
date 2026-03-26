// using LastMile.TMS.Application.Common.Interfaces;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;

// namespace LastMile.TMS.Persistence;

// public static class DependencyInjection
// {

//     public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
//     {
//         services.AddDbContextFactory<AppDbContext>((sp, options) =>
//             options.UseNpgsql(
//                 configuration.GetConnectionString("DefaultConnection"),
//                 npgsql =>
//                 {
//                     npgsql.UseNetTopologySuite();
//                     npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
//                 }),
//             ServiceLifetime.Scoped);

//         // AddDbContext registers AppDbContext as scoped AND wires up
//         // ICurrentUserService correctly through EF Core's internal DI handling
//         services.AddDbContext<AppDbContext>((sp, options) =>
//             options.UseNpgsql(
//                 configuration.GetConnectionString("DefaultConnection"),
//                 npgsql =>
//                 {
//                     npgsql.UseNetTopologySuite();
//                     npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
//                 }));

//         services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

//         return services;
//     }

//     // public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
//     // {
//     //     var npgsqlConfig = (DbContextOptionsBuilder options) =>
//     //         options.UseNpgsql(
//     //             configuration.GetConnectionString("DefaultConnection"),
//     //             npgsql =>
//     //             {
//     //                 npgsql.UseNetTopologySuite();
//     //                 npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
//     //             });

//     //     // Factory for HotChocolate resolvers — creates its own scope for ICurrentUserService
//     //     services.AddDbContextFactory<AppDbContext>((sp, options) =>
//     //     {
//     //         npgsqlConfig(options);
//     //     }, ServiceLifetime.Scoped);

//     //     // Scoped AppDbContext for Identity, OpenIddict, seeder, migrations
//     //     services.AddScoped<AppDbContext>(sp =>
//     //         sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

//     //     services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

//     //     return services;
//     // }
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
