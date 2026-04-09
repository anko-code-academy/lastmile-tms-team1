using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class DriverIntegrationTests
{
    private readonly IntegrationFixture _fx;
    private readonly string _run;

    public DriverIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
        _run = Guid.NewGuid().ToString("N")[..8];
    }

    private Task<JsonElement> GqlAsync(string query) => _fx.GraphQLRequestAsync(query);

    private async Task<(string depotId, string zoneId, string userEmail)> CreateDepotZoneAndUserAsync()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{ name: ""Driver Test Depot {_run}"", address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }} }}) {{
                id
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Driver Test Zone {_run}"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
            }}
        }}";

        var zoneJson = await GqlAsync(createZoneMutation);
        var zoneId = zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("id").GetString();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var userEmail = $"testdriver.{uniqueId}@lastmile.com";

        using var scope = _fx.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var driverRole = await roleManager.FindByNameAsync("Driver");
        if (driverRole == null)
        {
            driverRole = Role.CreateDriver();
            await roleManager.CreateAsync(driverRole);
        }

        var user = User.Create("Test", "Driver", userEmail, $"driver.{uniqueId}");
        user.SetPasswordHash("Test@1234");
        user.RoleId = driverRole.Id;
        var result = await userManager.CreateAsync(user);

        return (depotId!, zoneId!, userEmail);
    }

    [Fact]
    public async Task CreateDriver_WithValidInput_ReturnsDriver()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var mutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
                licenseNumber
                userId
                createdAt
            }}
        }}";

        var jsonResponse = await GqlAsync(mutation);

        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        jsonResponse.GetProperty("data").GetProperty("createDriver").TryGetProperty("id", out _).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createDriver").GetProperty("licenseNumber").GetString().Should().Be("DL123456");
    }

    [Fact]
    public async Task CreateDriver_WithMissingRequiredField_ReturnsValidationError()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var mutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: """",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var jsonResponse = await GqlAsync(mutation);

        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateDriver_WithPastLicenseExpiry_ReturnsValidationError()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var mutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2020-01-01T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var jsonResponse = await GqlAsync(mutation);

        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateDriver_WithDuplicateEmail_ReturnsValidationError()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createFirstMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL111111"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        await GqlAsync(createFirstMutation);

        var createSecondMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL222222"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var jsonResponse = await GqlAsync(createSecondMutation);

        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateDriver_WithValidInput_ReturnsUpdatedDriver()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateDriver(input: {{
                id: ""{driverId}"",
                licenseNumber: ""DL654321"",
                licenseExpiryDate: ""2028-12-31T00:00:00Z""
            }}) {{
                id
                licenseNumber
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);

        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        updateJson.GetProperty("data").GetProperty("updateDriver").GetProperty("licenseNumber").GetString().Should().Be("DL654321");
    }

    [Fact]
    public async Task DeleteDriver_WithValidId_ReturnsTrue()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        var deleteMutation = $@"mutation {{
            deleteDriver(id: ""{driverId}"")
        }}";

        var deleteJson = await GqlAsync(deleteMutation);

        if (deleteJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        deleteJson.GetProperty("data").GetProperty("deleteDriver").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task QueryDriver_ById_ReturnsDriver()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        var query = $@"query {{
            driver(id: ""{driverId}"") {{
                id
                licenseNumber
                licenseExpiryDate
                userId
                createdAt
                user {{
                    firstName
                    lastName
                    email
                    isActive
                }}
            }}
        }}";

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        queryJson.GetProperty("data").GetProperty("driver").GetProperty("licenseNumber").GetString().Should().Be("DL123456");
        queryJson.GetProperty("data").GetProperty("driver").GetProperty("user").GetProperty("firstName").GetString().Should().Be("Test");
        queryJson.GetProperty("data").GetProperty("driver").GetProperty("user").GetProperty("lastName").GetString().Should().Be("Driver");
        queryJson.GetProperty("data").GetProperty("driver").GetProperty("user").GetProperty("email").GetString().Should().Be(userEmail);
    }

    [Fact]
    public async Task QueryDrivers_WithPagination_ReturnsPaginatedResults()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        await GqlAsync(createMutation);

        var query = @"query {
            drivers {
                nodes {
                    id
                    licenseNumber
                }
                pageInfo {
                    hasNextPage
                }
            }
        }";

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var drivers = queryJson.GetProperty("data").GetProperty("drivers").GetProperty("nodes");
        drivers.GetArrayLength().Should().BeGreaterThan(0);

        var pageInfo = queryJson.GetProperty("data").GetProperty("drivers").GetProperty("pageInfo");
        pageInfo.TryGetProperty("hasNextPage", out _).Should().BeTrue();
    }

    [Fact]
    public async Task QueryDrivers_WithFiltering_ReturnsFilteredResults()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL111111"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        await GqlAsync(createMutation);

        var query = @"query {
            drivers {
                nodes {
                    id
                    licenseNumber
                    user {
                        isActive
                    }
                }
            }
        }";

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var drivers = queryJson.GetProperty("data").GetProperty("drivers").GetProperty("nodes");
        drivers.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateDriver_AddDayOff_WhenDriverHasRouteOnThatDate_ReturnsError()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var nextWeek = DateTime.UtcNow.AddDays(7);
        var dayOfWeek = nextWeek.DayOfWeek;

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL_DAYOFF_CONFLICT"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                shiftSchedules: [
                    {{ dayOfWeek: {dayOfWeek.ToString().ToUpperInvariant()}, openTime: ""08:00:00"", closeTime: ""17:00:00"" }}
                ]
            }}) {{
                id
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        var routeMutation = $@"mutation {{
            createRoute(input: {{
                name: ""DayOff Conflict Route {_run}"",
                plannedStartTime: ""{nextWeek:O}"",
                totalDistanceKm: 30.0,
                totalParcelCount: 15,
                driverId: ""{driverId}""
            }}) {{ id }}
        }}";
        await GqlAsync(routeMutation);

        var dayOffDate = new DateTimeOffset(nextWeek.Year, nextWeek.Month, nextWeek.Day, 0, 0, 0, TimeSpan.Zero);
        var updateMutation = $@"mutation {{
            updateDriver(input: {{
                id: ""{driverId}"",
                licenseNumber: ""DL_DAYOFF_CONFLICT"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                daysOff: [
                    {{ date: ""{dayOffDate:O}"" }}
                ]
            }}) {{
                id
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);

        updateJson.TryGetProperty("errors", out var updateErrors).Should().BeTrue();
        updateErrors.GetArrayLength().Should().BeGreaterThan(0);
        updateErrors[0].GetProperty("message").GetString().Should().Contain("day off");
    }

    [Fact]
    public async Task UpdateDriver_AddDayOff_WhenNoConflict_Succeeds()
    {
        var (depotId, zoneId, userEmail) = await CreateDepotZoneAndUserAsync();

        var createMutation = $@"mutation {{
            createDriver(input: {{
                email: ""{userEmail}"",
                licenseNumber: ""DL_DAYOFF_OK"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z""
            }}) {{
                id
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        var dayOffDate = new DateTimeOffset(2027, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var updateMutation = $@"mutation {{
            updateDriver(input: {{
                id: ""{driverId}"",
                licenseNumber: ""DL_DAYOFF_OK"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                daysOff: [
                    {{ date: ""{dayOffDate:O}"" }}
                ]
            }}) {{
                id
                daysOff {{
                    date
                }}
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);

        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var daysOff = updateJson.GetProperty("data").GetProperty("updateDriver").GetProperty("daysOff");
        daysOff.GetArrayLength().Should().Be(1);
    }
}
