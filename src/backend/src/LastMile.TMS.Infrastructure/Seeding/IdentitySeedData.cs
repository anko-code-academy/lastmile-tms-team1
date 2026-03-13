using LastMile.TMS.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LastMile.TMS.Infrastructure.Seeding;

public static class IdentitySeedData
{
    private static readonly string[] Roles =
    [
        "Admin",
        "Operations Manager",
        "Dispatcher",
        "Warehouse Operator",
        "Driver"
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, authOptions.Value.DefaultAdmin);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Roles)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<IdentityUser<Guid>> userManager, DefaultAdminOptions adminOptions)
    {
        var existingAdmin = await userManager.FindByNameAsync(adminOptions.UserName);
        if (existingAdmin != null)
            return;

        var admin = new IdentityUser<Guid>
        {
            UserName = adminOptions.UserName,
            Email = adminOptions.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminOptions.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create admin user: {errors}");
        }

        result = await userManager.AddToRoleAsync(admin, "Admin");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add admin role: {errors}");
        }
    }
}