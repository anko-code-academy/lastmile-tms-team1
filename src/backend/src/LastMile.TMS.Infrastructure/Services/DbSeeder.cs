using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LastMile.TMS.Infrastructure.Services;

public class DbSeeder : IDbSeeder
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<DbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // Seed roles
        await SeedRolesAsync();

        // Seed admin user
        await SeedAdminUserAsync();

        // Seed test users for each role (no zone/depot - assign manually via CRUD)
        await SeedTestUsersAsync();

        _logger.LogInformation("Database seeding completed.");
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
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    _logger.LogInformation("Created role: {RoleName}", role.Name);
                }
            }
            else
            {
                _logger.LogDebug("Role already exists: {RoleName}", role.Name);
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

        _logger.LogInformation("Seeding admin user: {Username}, {Email}", adminUsername, adminEmail);

        const string adminPhone = "+1234567890";

        // Check by username OR email
        var existingAdmin = await _userManager.FindByNameAsync(adminUsername);
        if (existingAdmin != null)
        {
            // Reset password to ensure it matches config
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingAdmin);
            var result = await _userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin user password reset successfully.");
            }
            else
            {
                _logger.LogWarning("Failed to reset admin password: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Fix phone if empty (avoid unique constraint issues)
            if (string.IsNullOrWhiteSpace(existingAdmin.PhoneNumber))
            {
                existingAdmin.UpdatePhone(adminPhone);
                await _userManager.UpdateAsync(existingAdmin);
                _logger.LogInformation("Updated admin phone number.");
            }

            // Ensure admin has Admin role
            if (!await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await _userManager.AddToRoleAsync(existingAdmin, "Admin");
                // Also set the RoleId foreign key since AddToRoleAsync doesn't do this
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    existingAdmin.RoleId = adminRole.Id;
                    existingAdmin.RoleName = adminRole.Name;
                    await _userManager.UpdateAsync(existingAdmin);
                }
                _logger.LogInformation("Added Admin role to existing user.");
            }

            // Ensure admin is marked as system admin
            if (!existingAdmin.IsSystemAdmin)
            {
                existingAdmin.MarkAsSystemAdmin();
                await _userManager.UpdateAsync(existingAdmin);
                _logger.LogInformation("Marked existing user as system admin.");
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
                _logger.LogInformation("Admin user (by email) password reset successfully.");
            }

            // Fix phone if empty (avoid unique constraint issues)
            if (string.IsNullOrWhiteSpace(existingByEmail.PhoneNumber))
            {
                existingByEmail.UpdatePhone(adminPhone);
                await _userManager.UpdateAsync(existingByEmail);
                _logger.LogInformation("Updated admin phone number.");
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
                // Also set the RoleId foreign key since AddToRoleAsync doesn't do this
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    existingByEmail.RoleId = adminRole.Id;
                    existingByEmail.RoleName = adminRole.Name;
                    await _userManager.UpdateAsync(existingByEmail);
                }
                _logger.LogInformation("Added Admin role to existing user.");
            }

            // Ensure admin is marked as system admin
            if (!existingByEmail.IsSystemAdmin)
            {
                existingByEmail.MarkAsSystemAdmin();
                await _userManager.UpdateAsync(existingByEmail);
                _logger.LogInformation("Marked existing user as system admin.");
            }
            return;
        }

        try
        {
            var admin = User.Create(
                "System",
                "Administrator",
                adminEmail,
                adminUsername,
                adminPhone);

            var result = await _userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                // Also set the RoleId foreign key since AddToRoleAsync doesn't do this
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    admin.RoleId = adminRole.Id;
                    admin.RoleName = adminRole.Name;
                }
                admin.MarkAsSystemAdmin();
                await _userManager.UpdateAsync(admin);
                _logger.LogInformation("Admin user created successfully with Admin role and marked as system admin.");
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating admin user");
        }
    }

    private async Task SeedTestUsersAsync()
    {
        // Test users - one per role (password: Test@1234)
        var testUsers = new[]
        {
            ("Operations Manager", "ops@lastmile.com", "Opmgr", "Test@1234", "Operations Manager", "+1234567891"),
            ("Dispatcher", "dispatcher@lastmile.com", "Dispatchr", "Test@1234", "Dispatcher", "+1234567892"),
            ("Warehouse Operator", "warehouse@lastmile.com", "Warehouseop", "Test@1234", "Warehouse Operator", "+1234567893"),
            ("Driver", "driver@lastmile.com", "Drivr", "Test@1234", "Driver", "+1234567894"),
        };

        foreach (var (firstName, email, userName, password, roleName, phone) in testUsers)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                // Update phone if it's empty (fix unique constraint issue)
                if (string.IsNullOrWhiteSpace(existing.PhoneNumber))
                {
                    existing.UpdatePhone(phone);
                    await _userManager.UpdateAsync(existing);
                    _logger.LogInformation("Updated phone for existing user {Email}", email);
                }

                // Ensure existing user has the correct role
                if (!await _userManager.IsInRoleAsync(existing, roleName))
                {
                    await _userManager.AddToRoleAsync(existing, roleName);
                    _logger.LogInformation("Assigned role {Role} to existing user {Email}", roleName, email);
                }
                else
                {
                    _logger.LogDebug("Test user already exists with correct role: {Email}", email);
                }

                // Always sync the RoleId foreign key to match Identity role
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null && existing.RoleId != role.Id)
                {
                    existing.RoleId = role.Id;
                    existing.RoleName = role.Name;
                    await _userManager.UpdateAsync(existing);
                    _logger.LogInformation("Synced RoleId foreign key for {Email}", email);
                }
                continue;
            }

            try
            {
                var user = User.Create(firstName, lastName: $"{roleName} User", email, userName, phone);
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                    // Also set the RoleId foreign key since AddToRoleAsync doesn't do this
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        user.RoleId = role.Id;
                        user.RoleName = role.Name;
                        await _userManager.UpdateAsync(user);
                    }
                    _logger.LogInformation("Created test user: {Email} with role {Role}", email, roleName);
                }
                else
                {
                    _logger.LogWarning("Failed to create test user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while creating test user {Email}", email);
            }
        }
    }
}
