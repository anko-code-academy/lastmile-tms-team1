using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Infrastructure.Services;

public class DbSeeder : IDbSeeder
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbSeeder> _logger;
    private static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public DbSeeder(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IAppDbContext context,
        IConfiguration configuration,
        ILogger<DbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
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

        // Seed depot and zone for Empire State Building
        await SeedDepotAndZoneAsync();

        // Seed aisles and bins (must run before parcels so sorted parcels can be assigned to bins)
        await SeedAislesAndBinsAsync();

        // Seed parcels
        await SeedParcelsAsync();

        // Seed driver profile — must run after SeedTestUsersAsync (depends on driver user)
        await SeedDriverAsync();

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

    private async Task SeedDepotAndZoneAsync()
    {
        const string empireStateDepotName = "Empire State Building Depot";
        const string zoneName = "Manhattan Zone";

        // Check if depot already exists
        var existingDepot = _context.Depots.FirstOrDefault(d => d.Name == empireStateDepotName);
        if (existingDepot != null)
        {
            _logger.LogDebug("Depot {DepotName} already exists, skipping seed", empireStateDepotName);
            return;
        }

        // Empire State Building coordinates (SRID 4326)
        const double empireStateLng = -73.985428;
        const double empireStateLat = 40.748817;

        // Create address
        var address = new Address
        {
            Street1 = "20 W 34th St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            IsResidential = false,
            GeoLocation = GeometryFactory.CreatePoint(new Coordinate(empireStateLng, empireStateLat))
        };

        // Create depot
        var depot = new Depot
        {
            Name = empireStateDepotName,
            Address = address,
            IsActive = true
        };

        // Create a polygon zone around Empire State Building (roughly 500m radius)
        // Coordinates forming a square around the Empire State Building
        const double offset = 0.0045; // roughly 500m in degrees
        var zonePolygon = GeometryFactory.CreatePolygon(new[]
        {
            new Coordinate(empireStateLng - offset, empireStateLat - offset),
            new Coordinate(empireStateLng + offset, empireStateLat - offset),
            new Coordinate(empireStateLng + offset, empireStateLat + offset),
            new Coordinate(empireStateLng - offset, empireStateLat + offset),
            new Coordinate(empireStateLng - offset, empireStateLat - offset) // close the ring
        });

        // Create zone
        var zone = new Zone
        {
            Name = zoneName,
            BoundaryGeometry = zonePolygon,
            IsActive = true,
            Depot = depot
        };

        // Add depot first (with its address), then zone separately
        _context.Depots.Add(depot);
        _context.Zones.Add(zone);
        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Seeded depot {DepotName} with zone {ZoneName} at Empire State Building",
            empireStateDepotName, zoneName);
    }

    private async Task SeedParcelsAsync()
    {
        // Skip if parcels already exist
        if (_context.Parcels.Any())
        {
            _logger.LogDebug("Parcels already exist, skipping seed");
            return;
        }

        // Get the existing depot and zone
        var depot = _context.Depots.FirstOrDefault(d => d.Name == "Empire State Building Depot");
        var zone = _context.Zones.FirstOrDefault(z => z.Name == "Manhattan Zone");
        var bins = _context.Bins.ToList();

        if (depot == null)
        {
            _logger.LogWarning("Depot not found, skipping parcel seeding");
            return;
        }

        // Create shipper address (warehouse at depot)
        var shipperAddress = new Address
        {
            Street1 = "20 W 34th St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            IsResidential = false,
            ContactName = "LastMile Warehouse",
            CompanyName = "LastMile Logistics",
            Phone = "+12125551234",
            GeoLocation = GeometryFactory.CreatePoint(new Coordinate(-73.985428, 40.748817))
        };

        _context.Addresses.Add(shipperAddress);

        // Recipient data pools
        var firstNames = new[]
        {
            "James", "Maria", "Robert", "Jennifer", "Michael", "Linda", "David", "Patricia",
            "William", "Elizabeth", "Richard", "Barbara", "Joseph", "Susan", "Thomas", "Jessica",
            "Charles", "Sarah", "Christopher", "Karen", "Daniel", "Nancy", "Matthew", "Lisa",
            "Anthony", "Betty", "Mark", "Margaret", "Donald", "Sandra", "Steven", "Ashley",
            "Paul", "Dorothy", "Andrew", "Kimberly", "Joshua", "Emily", "Kenneth", "Donna"
        };

        var lastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
            "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker",
            "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores"
        };

        var streets = new[]
        {
            "100 Broadway", "250 Park Ave", "55 Water St", "300 W 57th St", "150 E 34th St",
            "88 Greenwich St", "420 Lexington Ave", "760 3rd Ave", "200 Chambers St",
            "350 5th Ave", "1 Penn Plaza", "230 Park Ave", "450 W 33rd St",
            "600 3rd Ave", "1350 Broadway", "40 Wall St", "1 World Trade Center",
            "30 Rockefeller Plaza", "1271 6th Ave", "345 Park Ave South"
        };

        var cities = new[]
        {
            ("New York", "NY", "100"),
            ("Brooklyn", "NY", "112"),
            ("Queens", "NY", "113"),
            ("Bronx", "NY", "104"),
            ("Staten Island", "NY", "103"),
            ("Jersey City", "NJ", "073"),
            ("Hoboken", "NJ", "070"),
            ("Newark", "NJ", "071"),
        };

        var descriptions = new[]
        {
            "Electronics", "Clothing", "Books", "Food items", "Office supplies",
            "Medical supplies", "Furniture parts", "Personal effects", "Documents", "Gift package",
            "Auto parts", "Sporting goods", "Cosmetics", "Toys", "Household items",
        };

        var parcelTypes = Enum.GetValues<ParcelType>();
        var serviceTypes = Enum.GetValues<ServiceType>();
        var statuses = Enum.GetValues<ParcelStatus>();

        var random = new Random(42); // deterministic seed
        var recipientAddresses = new Address[100];

        // Create all recipient addresses first
        for (int i = 0; i < 100; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var street = streets[random.Next(streets.Length)];
            var (city, state, zipPrefix) = cities[random.Next(cities.Length)];

            var lat = 40.748817 + (random.NextDouble() - 0.5) * 0.08;
            var lng = -73.985428 + (random.NextDouble() - 0.5) * 0.08;

            var recipientAddress = new Address
            {
                Street1 = $"{random.Next(1, 9999)} {street.Split(' ', 2)[1]}",
                City = city,
                State = state,
                PostalCode = $"{zipPrefix}{random.Next(10, 99)}",
                CountryCode = "US",
                IsResidential = true,
                ContactName = $"{firstName} {lastName}",
                Phone = $"+1212555{i:D4}",
                GeoLocation = GeometryFactory.CreatePoint(new Coordinate(lng, lat))
            };

            recipientAddresses[i] = recipientAddress;
            _context.Addresses.Add(recipientAddress);
        }

        // Save all addresses (shipper + 100 recipients) in one round-trip
        await _context.SaveChangesAsync(CancellationToken.None);

        // Create all parcels
        var parcels = new List<Parcel>();

        for (int i = 0; i < 100; i++)
        {
            var description = descriptions[random.Next(descriptions.Length)];
            var serviceType = serviceTypes[random.Next(serviceTypes.Length)];

            var parcel = Parcel.Create($"{description} — order #{1000 + i}", serviceType);
            parcel.RecipientAddress = recipientAddresses[i];
            parcel.ShipperAddress = shipperAddress;
            parcel.Weight = Math.Round((decimal)(random.NextDouble() * 20) + 0.5m, 2);
            parcel.WeightUnit = WeightUnit.Lb;
            parcel.Length = Math.Round((decimal)(random.NextDouble() * 30) + 5, 1);
            parcel.Width = Math.Round((decimal)(random.NextDouble() * 20) + 5, 1);
            parcel.Height = Math.Round((decimal)(random.NextDouble() * 15) + 2, 1);
            parcel.DimensionUnit = DimensionUnit.In;
            parcel.DeclaredValue = Math.Round((decimal)(random.NextDouble() * 500) + 10, 2);
            parcel.ParcelType = parcelTypes[random.Next(parcelTypes.Length)];
            parcel.EstimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(random.Next(1, 7));

            // Distribute statuses realistically
            var statusIndex = i switch
            {
                < 15 => 0,  // Registered
                < 25 => 1,  // ReceivedAtDepot
                < 35 => 2,  // Sorted
                < 42 => 3,  // Staged
                < 48 => 4,  // Loaded
                < 55 => 5,  // OutForDelivery
                < 75 => 6,  // Delivered
                < 82 => 7,  // FailedAttempt
                < 88 => 8,  // ReturnedToDepot
                < 95 => 9,  // Cancelled
                _ => 10     // Exception
            };
            parcel.Status = statuses[statusIndex];

            if (parcel.Status == ParcelStatus.Sorted && bins.Count > 0)
            {
                parcel.BinId = bins[random.Next(bins.Count)].Id;
            }

            if (parcel.Status == ParcelStatus.Delivered)
            {
                parcel.ActualDeliveryDate = DateTimeOffset.UtcNow.AddDays(-random.Next(0, 5));
            }

            parcel.Zone = zone;
            parcel.DepotId = depot.Id;
            parcels.Add(parcel);
        }

        // Save all parcels in one round-trip
        _context.Parcels.AddRange(parcels);
        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Seeded {Count} parcels", parcels.Count);
    }

    private async Task SeedAislesAndBinsAsync()
    {
        // Skip if bins already exist
        if (_context.Bins.Any())
        {
            _logger.LogDebug("Bins already exist, skipping seed");
            return;
        }

        var zone = _context.Zones.FirstOrDefault(z => z.Name == "Manhattan Zone");
        if (zone == null)
        {
            _logger.LogWarning("Manhattan Zone not found, skipping aisle/bin seeding");
            return;
        }

        var depot = _context.Depots.FirstOrDefault(d => d.Name == "Empire State Building Depot");
        if (depot == null)
        {
            _logger.LogWarning("Empire State Building Depot not found, skipping aisle/bin seeding");
            return;
        }

        var depotFirstChar = depot.Name[0];
        var zoneFirstChar = zone.Name[0];

        // Create 3 aisles
        var aisles = new List<Aisle>();
        for (int i = 1; i <= 3; i++)
        {
            var aisle = new Aisle
            {
                Name = $"Aisle {i}",
                Order = i,
                IsActive = true,
                ZoneId = zone.Id
            };
            aisle.SetLabel(depotFirstChar.ToString(), zoneFirstChar.ToString());
            aisles.Add(aisle);
        }

        _context.Aisles.AddRange(aisles);
        await _context.SaveChangesAsync(CancellationToken.None);
        _logger.LogInformation("Seeded {Count} aisles", aisles.Count);

        // Create bins: 5 bins per aisle, capacity 20 each
        var bins = new List<Bin>();
        foreach (var aisle in aisles)
        {
            for (int slot = 1; slot <= 5; slot++)
            {
                var bin = new Bin
                {
                    Slot = slot,
                    Capacity = 20,
                    IsActive = true,
                    ZoneId = zone.Id,
                    AisleId = aisle.Id
                };
                bin.SetLabel(aisle.Label);
                bins.Add(bin);
            }
        }

        _context.Bins.AddRange(bins);
        await _context.SaveChangesAsync(CancellationToken.None);
        _logger.LogInformation("Seeded {Count} bins", bins.Count);
    }

    private async Task SeedDriverAsync()
    {
        const string driverEmail = "driver@lastmile.com";
        const string driverLicenseNumber = "DL-2026-001";

        // Check if driver already exists by license number
        var existingDriver = _context.Drivers.FirstOrDefault(d => d.LicenseNumber == driverLicenseNumber);
        if (existingDriver != null)
        {
            _logger.LogDebug("Driver with license {LicenseNumber} already exists, skipping seed", driverLicenseNumber);
            return;
        }

        // Find the test driver user
        var driverUser = await _userManager.FindByEmailAsync(driverEmail);
        if (driverUser == null)
        {
            _logger.LogWarning("Driver user {Email} not found, skipping driver seed", driverEmail);
            return;
        }

        // Create driver profile
        var driver = new Driver
        {
            LicenseNumber = driverLicenseNumber,
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(2),
            UserId = driverUser.Id
        };

        // Add shift schedules (Mon–Sun, 8:00–18:00)
        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            driver.ShiftSchedules.Add(new ShiftSchedule
            {
                DayOfWeek = day,
                OpenTime = new TimeOnly(8, 0),
                CloseTime = new TimeOnly(18, 0),
                Driver = driver
            });
        }

        // Add a day off (next Sunday)
        var nextSunday = DateTimeOffset.UtcNow.AddDays(DayOfWeek.Sunday - DateTimeOffset.UtcNow.DayOfWeek);
        driver.DaysOff.Add(new DayOff
        {
            Date = nextSunday,
            Driver = driver
        });

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Seeded driver {LicenseNumber} for user {Email} with 7 shift schedules and 1 day off",
            driverLicenseNumber, driverEmail);
    }
}
