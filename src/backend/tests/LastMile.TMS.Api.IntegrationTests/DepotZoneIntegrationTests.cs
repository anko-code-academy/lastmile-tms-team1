using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class DepotZoneIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;

    public DepotZoneIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
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

    [Fact]
    public async Task CreateDepot_WithValidInput_ReturnsDepot()
    {
        // Arrange
        var mutation = @"mutation {
            createDepot(input: {
                name: ""Test Depot"",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" },
                isActive: true
            }) {
                id
                name
                isActive
                createdAt
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.GetProperty("data").GetProperty("createDepot").TryGetProperty("id", out var id).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createDepot").GetProperty("name").GetString().Should().Be("Test Depot");
        jsonResponse.GetProperty("data").GetProperty("createDepot").GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateDepot_WithMissingName_ReturnsValidationError()
    {
        // Arrange
        var mutation = @"mutation {
            createDepot(input: {
                name: """",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }
            }) {
                id
                name
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateDepot_WithValidInput_ReturnsUpdatedDepot()
    {
        // Arrange - First create a depot
        var createMutation = @"mutation {
            createDepot(input: {
                name: ""Original Depot"",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }
            }) {
                id
                name
            }
        }";

        var createJson = await GraphQLRequestAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Act - Update the depot
        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Updated Depot"",
                address: {{ street1: ""456 Update St"", city: ""Update City"", state: ""US"", postalCode: ""54321"", countryCode: ""US"" }},
                isActive: false
            }}) {{
                id
                name
                isActive
            }}
        }}";

        var updateJson = await GraphQLRequestAsync(updateMutation);

        // Assert
        updateJson.GetProperty("data").GetProperty("updateDepot").GetProperty("name").GetString().Should().Be("Updated Depot");
        updateJson.GetProperty("data").GetProperty("updateDepot").GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task CreateZone_WithValidGeoJson_ReturnsZone()
    {
        // Arrange - First create a depot
        var createDepotMutation = @"mutation {
            createDepot(input: { name: ""Zone Test Depot"", address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" } }) {
                id
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Arrange - Create zone with valid GeoJSON polygon
        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var mutation = $@"mutation {{
            createZone(input: {{
                name: ""Test Zone"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
                depotId
                isActive
                createdAt
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.GetProperty("data").GetProperty("createZone").TryGetProperty("id", out var zoneId).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createZone").GetProperty("name").GetString().Should().Be("Test Zone");
        jsonResponse.GetProperty("data").GetProperty("createZone").GetProperty("depotId").GetString().Should().Be(depotId);
    }

    [Fact]
    public async Task CreateZone_WithInvalidGeoJson_ReturnsValidationError()
    {
        // Arrange - First create a depot
        var createDepotMutation = @"mutation {
            createDepot(input: { name: ""Invalid GeoJSON Depot"", address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" } }) {
                id
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Arrange - Create zone with invalid GeoJSON (point instead of polygon)
        var invalidGeoJson = @"{""type"":""Point"",""coordinates"":[-122.4194,37.7749]}";
        var mutation = $@"mutation {{
            createZone(input: {{
                name: ""Invalid Zone"",
                geoJson: ""{invalidGeoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateZone_LinkedToDepot_ReturnsZoneWithDepot()
    {
        // Arrange - Create depot first
        var createDepotMutation = @"mutation {
            createDepot(input: { name: ""Link Test Depot"", address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" } }) {
                id
                name
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Arrange - Create zone with GeoJSON polygon
        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Linked Zone"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
                depotId
            }}
        }}";

        // Act
        var zoneJson = await GraphQLRequestAsync(createZoneMutation);

        // Assert - Verify zone was created with correct depot link
        zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("depotId").GetString().Should().Be(depotId);
    }

    [Fact]
    public async Task UpdateZone_WithValidInput_ReturnsUpdatedZone()
    {
        // Arrange - Create depot first
        var createDepotMutation = @"mutation {
            createDepot(input: { name: ""Update Zone Depot"", address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" } }) {
                id
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Arrange - Create zone
        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Original Zone"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
            }}
        }}";

        var zoneJson = await GraphQLRequestAsync(createZoneMutation);
        var zoneId = zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("id").GetString();

        // Act - Update the zone
        var updateMutation = $@"mutation {{
            updateZone(input: {{
                id: ""{zoneId}"",
                name: ""Updated Zone"",
                depotId: ""{depotId}"",
                isActive: false
            }}) {{
                id
                name
                isActive
            }}
        }}";

        var updateJson = await GraphQLRequestAsync(updateMutation);

        // Assert
        updateJson.GetProperty("data").GetProperty("updateZone").GetProperty("name").GetString().Should().Be("Updated Zone");
        updateJson.GetProperty("data").GetProperty("updateZone").GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task QueryDepot_ById_ReturnsDepot()
    {
        // Arrange - Create a depot
        var createMutation = @"mutation {
            createDepot(input: {
                name: ""Query Test Depot"",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" },
                isActive: true
            }) {
                id
                name
            }
        }";

        var createJson = await GraphQLRequestAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Act - Query the depot by ID
        var query = $@"query {{
            depot(id: ""{depotId}"") {{
                id
                name
                isActive
                createdAt
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        // Debug: print response if errors
        if (queryJson.TryGetProperty("errors", out _))
        {
            throw new Exception($"GraphQL errors: {queryJson.GetProperty("errors").GetRawText()}");
        }

        // Assert
        queryJson.GetProperty("data").GetProperty("depot").GetProperty("name").GetString().Should().Be("Query Test Depot");
        queryJson.GetProperty("data").GetProperty("depot").GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task QueryZones_ReturnsAllZones()
    {
        // Arrange - Create depot and zone
        var createDepotMutation = @"mutation {
            createDepot(input: { name: ""All Zones Depot"", address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" } }) {
                id
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Test Zone 1"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
            }}
        }}";

        await GraphQLRequestAsync(createZoneMutation);

        // Act - Query all zones
        var query = @"query {
            zones {
                nodes {
                    id
                    name
                    depotId
                    isActive
                }
            }
        }";

        var queryJson = await GraphQLRequestAsync(query);

        // Debug: print response if errors
        if (queryJson.TryGetProperty("errors", out _))
        {
            throw new Exception($"GraphQL errors: {queryJson.GetProperty("errors").GetRawText()}");
        }

        // Assert
        var zones = queryJson.GetProperty("data").GetProperty("zones").GetProperty("nodes");
        zones.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateDepot_CanBeQueriedById_AfterCreation()
    {
        // Arrange - Create a depot
        var createMutation = @"mutation {
            createDepot(input: {
                name: ""Persist Test Depot"",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" },
                isActive: true
            }) {
                id
                name
            }
        }";

        var createJson = await GraphQLRequestAsync(createMutation);

        if (createJson.TryGetProperty("errors", out var createErrors))
        {
            throw new Exception($"Create depot failed: {createErrors.GetRawText()}");
        }

        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();
        depotId.Should().NotBeNullOrEmpty("depot should be created with an ID");

        // Act - Query all depots and verify the created depot exists
        var query = @"query {
            depots {
                nodes {
                    id
                    name
                }
            }
        }";

        var queryJson = await GraphQLRequestAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query depots failed: {queryErrors.GetRawText()}");
        }

        // Assert - Find the created depot in the query results
        var depots = queryJson.GetProperty("data").GetProperty("depots").GetProperty("nodes");
        var depotIds = depots.EnumerateArray().Select(d => d.GetProperty("id").GetString()).ToList();

        depotIds.Should().Contain(depotId, $"Created depot with ID {depotId} should be queryable after creation. Found IDs: {string.Join(", ", depotIds)}");
    }

    [Fact]
    public async Task UpdateDepot_Address_ShouldPersistNewAddress()
    {
        // Arrange - Create depot with initial address
        var createMutation = @"mutation {
            createDepot(input: {
                name: ""Address Update Depot"",
                address: { street1: ""100 Old St"", city: ""Old City"", state: ""OC"", postalCode: ""11111"", countryCode: ""US"" }
            }) {
                id
                name
            }
        }";

        var createJson = await GraphQLRequestAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Act - Update the depot with a new address
        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Address Update Depot"",
                address: {{ street1: ""200 New St"", city: ""New City"", state: ""NC"", postalCode: ""22222"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        var updateJson = await GraphQLRequestAsync(updateMutation);

        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update depot failed: {errors.GetRawText()}");
        }

        // Assert - Query the depot and verify the new address
        var query = $@"query {{
            depot(id: ""{depotId}"") {{
                id
                name
                address {{
                    street1
                    city
                    state
                    postalCode
                }}
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query depot failed: {queryErrors.GetRawText()}");
        }

        var depot = queryJson.GetProperty("data").GetProperty("depot");
        depot.GetProperty("address").GetProperty("street1").GetString().Should().Be("200 New St");
        depot.GetProperty("address").GetProperty("city").GetString().Should().Be("New City");
        depot.GetProperty("address").GetProperty("state").GetString().Should().Be("NC");
        depot.GetProperty("address").GetProperty("postalCode").GetString().Should().Be("22222");
    }

    [Fact]
    public async Task UpdateDepot_Schedule_ShouldPersistNewSchedule()
    {
        // Arrange - Create depot with default schedule (Mon-Fri 9-17)
        var createMutation = @"mutation {
            createDepot(input: {
                name: ""Schedule Update Depot"",
                address: { street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }
            }) {
                id
                name
            }
        }";

        var createJson = await GraphQLRequestAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Act - Update the depot schedule to Mon-Sat 8-20
        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Schedule Update Depot"",
                address: {{ street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }},
                operatingHours: [
                    {{ dayOfWeek: MONDAY, openTime: ""08:00:00"", closeTime: ""20:00:00"" }},
                    {{ dayOfWeek: TUESDAY, openTime: ""08:00:00"", closeTime: ""20:00:00"" }},
                    {{ dayOfWeek: WEDNESDAY, openTime: ""08:00:00"", closeTime: ""20:00:00"" }},
                    {{ dayOfWeek: THURSDAY, openTime: ""08:00:00"", closeTime: ""20:00:00"" }},
                    {{ dayOfWeek: FRIDAY, openTime: ""08:00:00"", closeTime: ""20:00:00"" }},
                    {{ dayOfWeek: SATURDAY, openTime: ""08:00:00"", closeTime: ""20:00:00"" }}
                ],
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        var updateJson = await GraphQLRequestAsync(updateMutation);

        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update depot schedule failed: {errors.GetRawText()}");
        }

        // Assert - Query the depot and verify the new schedule
        var query = $@"query {{
            depot(id: ""{depotId}"") {{
                id
                name
                shiftSchedules {{
                    dayOfWeek
                    openTime
                    closeTime
                }}
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query depot failed: {queryErrors.GetRawText()}");
        }

        var depot = queryJson.GetProperty("data").GetProperty("depot");
        var schedules = depot.GetProperty("shiftSchedules");

        // Verify Saturday is now in the schedule with new hours
        var saturdaySchedule = schedules.EnumerateArray().FirstOrDefault(s => s.GetProperty("dayOfWeek").GetString() == "SATURDAY");
        saturdaySchedule.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        saturdaySchedule.GetProperty("openTime").GetString().Should().Be("08:00:00");
        saturdaySchedule.GetProperty("closeTime").GetString().Should().Be("20:00:00");

        // Verify Friday has updated hours (was 17:00, now 20:00)
        var fridaySchedule = schedules.EnumerateArray().FirstOrDefault(s => s.GetProperty("dayOfWeek").GetString() == "FRIDAY");
        fridaySchedule.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        fridaySchedule.GetProperty("openTime").GetString().Should().Be("08:00:00");
        fridaySchedule.GetProperty("closeTime").GetString().Should().Be("20:00:00");

        // Verify Sunday is NOT in the schedule anymore
        var sundaySchedule = schedules.EnumerateArray().FirstOrDefault(s => s.GetProperty("dayOfWeek").GetString() == "SUNDAY");
        sundaySchedule.ValueKind.Should().Be(JsonValueKind.Undefined);
    }

    [Fact]
    public async Task UpdateDepot_Name_ZoneDepotName_ShouldReflectChange()
    {
        // Arrange - Create depot
        var createDepotMutation = @"mutation {
            createDepot(input: {
                name: ""Original Depot Name"",
                address: { street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }
            }) {
                id
                name
            }
        }";

        var depotJson = await GraphQLRequestAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        // Arrange - Create zone linked to the depot
        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Test Zone"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
            }}
        }}";

        await GraphQLRequestAsync(createZoneMutation);

        // Act - Update depot name
        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Updated Depot Name"",
                address: {{ street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        await GraphQLRequestAsync(updateMutation);

        // Assert - Query the zone and verify depot name has changed
        var query = $@"query {{
            zones {{
                nodes {{
                    id
                    name
                    depot {{
                        id
                        name
                    }}
                }}
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query zones failed: {queryErrors.GetRawText()}");
        }

        var zones = queryJson.GetProperty("data").GetProperty("zones").GetProperty("nodes");
        var testZone = zones.EnumerateArray().FirstOrDefault(z => z.GetProperty("name").GetString() == "Test Zone");

        testZone.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        testZone.GetProperty("depot").GetProperty("name").GetString().Should().Be("Updated Depot Name");
    }

    private static async Task CleanupTestDataAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Delete test data - order matters due to FK constraints (zones reference depots)
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
