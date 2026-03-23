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
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
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
        var content = new StringContent(JsonSerializer.Serialize(new { query = mutation }), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

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
        var content = new StringContent(JsonSerializer.Serialize(new { query = mutation }), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Should have errors due to empty name
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

        var createContent = new StringContent(JsonSerializer.Serialize(new { query = createMutation }), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/graphql", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createJson = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
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

        var updateContent = new StringContent(JsonSerializer.Serialize(new { query = updateMutation }), Encoding.UTF8, "application/json");
        var updateResponse = await _client.PostAsync("/graphql", updateContent);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updateJson = JsonSerializer.Deserialize<JsonElement>(updateResponseContent);

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

        var depotContent = new StringContent(JsonSerializer.Serialize(new { query = createDepotMutation }), Encoding.UTF8, "application/json");
        var depotResponse = await _client.PostAsync("/graphql", depotContent);
        var depotResponseContent = await depotResponse.Content.ReadAsStringAsync();
        var depotJson = JsonSerializer.Deserialize<JsonElement>(depotResponseContent);
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
        var content = new StringContent(JsonSerializer.Serialize(new { query = mutation }), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

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

        var depotContent = new StringContent(JsonSerializer.Serialize(new { query = createDepotMutation }), Encoding.UTF8, "application/json");
        var depotResponse = await _client.PostAsync("/graphql", depotContent);
        var depotResponseContent = await depotResponse.Content.ReadAsStringAsync();
        var depotJson = JsonSerializer.Deserialize<JsonElement>(depotResponseContent);
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
        var content = new StringContent(JsonSerializer.Serialize(new { query = mutation }), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Should have errors due to invalid GeoJSON
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

        var depotContent = new StringContent(JsonSerializer.Serialize(new { query = createDepotMutation }), Encoding.UTF8, "application/json");
        var depotResponse = await _client.PostAsync("/graphql", depotContent);
        var depotResponseContent = await depotResponse.Content.ReadAsStringAsync();
        var depotJson = JsonSerializer.Deserialize<JsonElement>(depotResponseContent);
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

        var zoneContent = new StringContent(JsonSerializer.Serialize(new { query = createZoneMutation }), Encoding.UTF8, "application/json");
        var zoneResponse = await _client.PostAsync("/graphql", zoneContent);

        // Assert - Verify zone was created with correct depot link
        zoneResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var zoneResponseContent = await zoneResponse.Content.ReadAsStringAsync();
        var zoneJson = JsonSerializer.Deserialize<JsonElement>(zoneResponseContent);
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

        var depotContent = new StringContent(JsonSerializer.Serialize(new { query = createDepotMutation }), Encoding.UTF8, "application/json");
        var depotResponse = await _client.PostAsync("/graphql", depotContent);
        var depotResponseContent = await depotResponse.Content.ReadAsStringAsync();
        var depotJson = JsonSerializer.Deserialize<JsonElement>(depotResponseContent);
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

        var zoneContent = new StringContent(JsonSerializer.Serialize(new { query = createZoneMutation }), Encoding.UTF8, "application/json");
        var zoneResponse = await _client.PostAsync("/graphql", zoneContent);
        var zoneResponseContent = await zoneResponse.Content.ReadAsStringAsync();
        var zoneJson = JsonSerializer.Deserialize<JsonElement>(zoneResponseContent);
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

        var updateContent = new StringContent(JsonSerializer.Serialize(new { query = updateMutation }), Encoding.UTF8, "application/json");
        var updateResponse = await _client.PostAsync("/graphql", updateContent);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updateJson = JsonSerializer.Deserialize<JsonElement>(updateResponseContent);

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

        var createContent = new StringContent(JsonSerializer.Serialize(new { query = createMutation }), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/graphql", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createJson = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
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

        var queryContent = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        var queryResponse = await _client.PostAsync("/graphql", queryContent);

        // Assert
        queryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var queryResponseContent = await queryResponse.Content.ReadAsStringAsync();
        var queryJson = JsonSerializer.Deserialize<JsonElement>(queryResponseContent);

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

        var depotContent = new StringContent(JsonSerializer.Serialize(new { query = createDepotMutation }), Encoding.UTF8, "application/json");
        var depotResponse = await _client.PostAsync("/graphql", depotContent);
        var depotResponseContent = await depotResponse.Content.ReadAsStringAsync();
        var depotJson = JsonSerializer.Deserialize<JsonElement>(depotResponseContent);
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

        var zoneContent = new StringContent(JsonSerializer.Serialize(new { query = createZoneMutation }), Encoding.UTF8, "application/json");
        await _client.PostAsync("/graphql", zoneContent);

        // Act - Query all zones
        var query = @"query {
            zones {
                id
                name
                depotId
                isActive
            }
        }";

        var queryContent = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        var queryResponse = await _client.PostAsync("/graphql", queryContent);

        // Assert
        queryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var queryResponseContent = await queryResponse.Content.ReadAsStringAsync();
        var queryJson = JsonSerializer.Deserialize<JsonElement>(queryResponseContent);

        var zones = queryJson.GetProperty("data").GetProperty("zones");
        zones.GetArrayLength().Should().BeGreaterThan(0);
    }
}
