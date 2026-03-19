using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace LastMile.TMS.Api.Services;

public class DbSeeder : IDbSeeder
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public DbSeeder(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        Log.Information("Starting database seeding...");

        // Seed roles
        await SeedRolesAsync();

        // Seed admin user
        await SeedAdminUserAsync();

        Log.Information("Database seeding completed.");
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            Role.CreateAdmin(),
            Role.CreateOperationsManager(),
            Role.CreateDispatcher(),
            Role.CreateWarehouseOperator(),
            Role.CreateDriver()
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create role {RoleName}: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    Log.Information("Created role: {RoleName}", role.Name);
                }
            }
            else
            {
                Log.Debug("Role already exists: {RoleName}", role.Name);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = _configuration["AdminCredentials:Email"]
            ?? throw new InvalidOperationException("AdminCredentials:Email is not configured");
        var adminUsername = _configuration["AdminCredentials:Username"]
            ?? throw new InvalidOperationException("AdminCredentials:Username is not configured");
        var adminPassword = _configuration["AdminCredentials:Password"]
            ?? throw new InvalidOperationException("AdminCredentials:Password is not configured");

        Log.Information("Seeding admin user: {Username}, {Email}", adminUsername, adminEmail);

        // Check by username OR email
        var existingAdmin = await _userManager.FindByNameAsync(adminUsername);
        if (existingAdmin != null)
        {
            // Reset password to ensure it matches config
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingAdmin);
            var result = await _userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);
            if (result.Succeeded)
            {
                Log.Information("Admin user password reset successfully.");
            }
            else
            {
                Log.Warning("Failed to reset admin password: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Ensure admin has Admin role
            if (!await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await _userManager.AddToRoleAsync(existingAdmin, "Admin");
                Log.Information("Added Admin role to existing user.");
            }
            return;
        }

        var existingByEmail = await _userManager.FindByEmailAsync(adminEmail);
        if (existingByEmail != null)
        {
            // Reset password and update username
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingByEmail);
            var result = await _userManager.ResetPasswordAsync(existingByEmail, token, adminPassword);
            if (result.Succeeded)
            {
                Log.Information("Admin user (by email) password reset successfully.");
            }

            // Update username if different
            if (existingByEmail.UserName != adminUsername)
            {
                existingByEmail.UserName = adminUsername;
                await _userManager.UpdateAsync(existingByEmail);
            }

            // Ensure admin has Admin role
            if (!await _userManager.IsInRoleAsync(existingByEmail, "Admin"))
            {
                await _userManager.AddToRoleAsync(existingByEmail, "Admin");
                Log.Information("Added Admin role to existing user.");
            }
            return;
        }

        try
        {
            var admin = User.Create(
                "System",
                "Administrator",
                adminEmail,
                adminUsername);

            var result = await _userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                Log.Information("Admin user created successfully with Admin role.");
            }
            else
            {
                Log.Error("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception while creating admin user");
        }
    }
}