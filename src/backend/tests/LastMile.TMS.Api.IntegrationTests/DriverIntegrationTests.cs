using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class DriverIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;

    public DriverIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
    {
        _factory = new IntegrationTestWebApplicationFactory(postgreSqlFixture);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();

        // Clean up data from previous test runs BEFORE seeding (to ensure fresh state)
        await CleanupTestDataAsync(_factory.GetConnectionString());

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbSeeder = scope.ServiceProvider.GetRequiredService<LastMile.TMS.Application.Common.Interfaces.IDbSeeder>();
        await dbSeeder.SeedAsync();

        // Login to get access token
        var username = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

        var tokenResponse = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        }));

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenContent);

        if (!tokenResponse.IsSuccessStatusCode || !tokenJson.TryGetProperty("access_token", out _))
        {
            throw new Exception($"Token request failed: {tokenContent}");
        }

        _accessToken = tokenJson.GetProperty("access_token").GetString()!;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<JsonElement> GraphQLRequestAsync(string query)
    {
        var content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql") { Content = content };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseContent);
    }

    private async Task<(string depotId, string zoneId, string userId)> CreateDepotZoneAndUserAsync()
    {
        // Create depot
        var createDepotMutation = @"mutation {
            createDepot(input: { name: ""Driver Test Depot"", address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" } }) {
                id
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Create zone
        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Driver Test Zone"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
            }}
        }}";

        var zoneJson = await GraphQLRequestAsync(createZoneMutation);
        var zoneId = zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("id").GetString();

        // Note: User creation would require a separate flow; for now we assume UserId exists in seeded data
        // The seeded admin user should have a UserId - we'll use a fixed GUID approach
        var userId = "00000000-0000-0000-0000-000000000001";

        return (depotId!, zoneId!, userId);
    }

    [Fact]
    public async Task CreateDriver_WithValidInput_ReturnsDriver()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();

        var mutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""John"",
                lastName: ""Doe"",
                phone: ""+1-555-0100"",
                email: ""john.doe.test@example.com"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}"",
                isActive: true
            }}) {{
                id
                firstName
                lastName
                email
                isActive
                createdAt
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        jsonResponse.GetProperty("data").GetProperty("createDriver").TryGetProperty("id", out var id).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createDriver").GetProperty("firstName").GetString().Should().Be("John");
        jsonResponse.GetProperty("data").GetProperty("createDriver").GetProperty("lastName").GetString().Should().Be("Doe");
        jsonResponse.GetProperty("data").GetProperty("createDriver").GetProperty("email").GetString().Should().Be("john.doe.test@example.com");
        jsonResponse.GetProperty("data").GetProperty("createDriver").GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateDriver_WithMissingRequiredField_ReturnsValidationError()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();

        var mutation = $@"mutation {{
            createDriver(input: {{
                firstName: """",
                lastName: ""Doe"",
                phone: ""+1-555-0100"",
                email: ""john.doe@example.com"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}""
            }}) {{
                id
                firstName
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateDriver_WithPastLicenseExpiry_ReturnsValidationError()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();

        var mutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""John"",
                lastName: ""Doe"",
                phone: ""+1-555-0100"",
                email: ""past.expiry@example.com"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2020-01-01T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}""
            }}) {{
                id
                firstName
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateDriver_WithDuplicateEmail_ReturnsValidationError()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();
        var duplicateEmail = $"duplicate.{Guid.NewGuid()}@example.com";

        // Create first driver
        var createFirstMutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""John"",
                lastName: ""Doe"",
                phone: ""+1-555-0100"",
                email: ""{duplicateEmail}"",
                licenseNumber: ""DL111111"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}""
            }}) {{
                id
                email
            }}
        }}";

        await GraphQLRequestAsync(createFirstMutation);

        // Try to create second driver with same email
        var createSecondMutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""Jane"",
                lastName: ""Smith"",
                phone: ""+1-555-0101"",
                email: ""{duplicateEmail}"",
                licenseNumber: ""DL222222"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}""
            }}) {{
                id
                email
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(createSecondMutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateDriver_WithValidInput_ReturnsUpdatedDriver()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();

        // Create driver first
        var createMutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""John"",
                lastName: ""Doe"",
                phone: ""+1-555-0100"",
                email: ""update.test.{Guid.NewGuid()}@example.com"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}"",
                isActive: true
            }}) {{
                id
                firstName
                lastName
            }}
        }}";

        var createJson = await GraphQLRequestAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        // Act - Update the driver
        var updateMutation = $@"mutation {{
            updateDriver(input: {{
                id: ""{driverId}"",
                firstName: ""John Updated"",
                lastName: ""Doe Updated"",
                phone: ""+1-555-9999"",
                email: ""updated.email.{Guid.NewGuid()}@example.com"",
                licenseNumber: ""DL654321"",
                licenseExpiryDate: ""2028-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}"",
                isActive: false
            }}) {{
                id
                firstName
                lastName
                phone
                isActive
            }}
        }}";

        var updateJson = await GraphQLRequestAsync(updateMutation);

        // Assert
        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        updateJson.GetProperty("data").GetProperty("updateDriver").GetProperty("firstName").GetString().Should().Be("John Updated");
        updateJson.GetProperty("data").GetProperty("updateDriver").GetProperty("lastName").GetString().Should().Be("Doe Updated");
        updateJson.GetProperty("data").GetProperty("updateDriver").GetProperty("phone").GetString().Should().Be("+1-555-9999");
        updateJson.GetProperty("data").GetProperty("updateDriver").GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDriver_WithValidId_ReturnsTrue()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();

        // Create driver first
        var createMutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""Delete Me"",
                lastName: ""Driver"",
                phone: ""+1-555-0100"",
                email: ""delete.test.{Guid.NewGuid()}@example.com"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}"",
                isActive: true
            }}) {{
                id
            }}
        }}";

        var createJson = await GraphQLRequestAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        // Act - Delete the driver
        var deleteMutation = $@"mutation {{
            deleteDriver(id: ""{driverId}"")
        }}";

        var deleteJson = await GraphQLRequestAsync(deleteMutation);

        // Assert
        if (deleteJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        deleteJson.GetProperty("data").GetProperty("deleteDriver").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task QueryDriver_ById_ReturnsDriver()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();
        var testEmail = $"query.test.{Guid.NewGuid()}@example.com";

        // Create driver
        var createMutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""Query"",
                lastName: ""Test"",
                phone: ""+1-555-0100"",
                email: ""{testEmail}"",
                licenseNumber: ""DL123456"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}"",
                isActive: true
            }}) {{
                id
                firstName
                lastName
            }}
        }}";

        var createJson = await GraphQLRequestAsync(createMutation);
        var driverId = createJson.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        // Act - Query the driver by ID
        var query = $@"query {{
            driver(id: ""{driverId}"") {{
                id
                firstName
                lastName
                email
                phone
                isActive
                createdAt
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        // Assert
        if (queryJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        queryJson.GetProperty("data").GetProperty("driver").GetProperty("firstName").GetString().Should().Be("Query");
        queryJson.GetProperty("data").GetProperty("driver").GetProperty("lastName").GetString().Should().Be("Test");
        queryJson.GetProperty("data").GetProperty("driver").GetProperty("email").GetString().Should().Be(testEmail);
        queryJson.GetProperty("data").GetProperty("driver").GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task QueryDrivers_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();

        // Create multiple drivers
        for (int i = 1; i <= 3; i++)
        {
            var createMutation = $@"mutation {{
                createDriver(input: {{
                    firstName: ""Paginate{i}"",
                    lastName: ""Driver{i}"",
                    phone: ""+1-555-010{i}"",
                    email: ""paginate.test.{i}.{Guid.NewGuid()}@example.com"",
                    licenseNumber: ""DL{i}23456"",
                    licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                    zoneId: ""{zoneId}"",
                    depotId: ""{depotId}"",
                    userId: ""{userId}"",
                    isActive: true
                }}) {{
                    id
                }}
            }}";

            await GraphQLRequestAsync(createMutation);
        }

        // Act - Query drivers with pagination
        var query = @"query {
            drivers {
                nodes {
                    id
                    firstName
                    lastName
                }
                pageInfo {
                    hasNextPage
                }
            }
        }";

        var queryJson = await GraphQLRequestAsync(query);

        // Assert
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
        // Arrange
        var (depotId, zoneId, userId) = await CreateDepotZoneAndUserAsync();
        var filterEmail = $"filter.test.active.{Guid.NewGuid()}@example.com";

        // Create an active driver
        var createActiveMutation = $@"mutation {{
            createDriver(input: {{
                firstName: ""FilterActive"",
                lastName: ""Driver"",
                phone: ""+1-555-0100"",
                email: ""{filterEmail}"",
                licenseNumber: ""DL111111"",
                licenseExpiryDate: ""2027-12-31T00:00:00Z"",
                zoneId: ""{zoneId}"",
                depotId: ""{depotId}"",
                userId: ""{userId}"",
                isActive: true
            }}) {{
                id
            }}
        }}";

        await GraphQLRequestAsync(createActiveMutation);

        // Act - Query with isActive filter
        var query = $@"query {{
            drivers(where: {{ isActive: {{ eq: true }} }}) {{
                nodes {{
                    id
                    firstName
                    isActive
                }}
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        // Assert
        if (queryJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var drivers = queryJson.GetProperty("data").GetProperty("drivers").GetProperty("nodes");
        var activeDrivers = drivers.EnumerateArray().Where(d => d.GetProperty("isActive").GetBoolean() == true).ToList();
        activeDrivers.Should().NotBeEmpty();
    }

    private static async Task CleanupTestDataAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Delete test data - order matters due to FK constraints
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Drivers\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Zones\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Depots\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
