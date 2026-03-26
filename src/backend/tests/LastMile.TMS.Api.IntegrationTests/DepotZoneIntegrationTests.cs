using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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
                name: """"
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
                name: ""Original Depot""
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
            createDepot(input: { name: ""Zone Test Depot"" }) {
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
            createDepot(input: { name: ""Invalid GeoJSON Depot"" }) {
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
            createDepot(input: { name: ""Link Test Depot"" }) {
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
            createDepot(input: { name: ""Update Zone Depot"" }) {
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
            createDepot(input: { name: ""All Zones Depot"" }) {
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
}
