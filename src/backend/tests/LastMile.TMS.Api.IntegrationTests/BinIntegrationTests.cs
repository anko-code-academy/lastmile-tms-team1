using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class BinIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;
    private string _depotId = null!;
    private string _zoneId = null!;

    public BinIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
    {
        _factory = new IntegrationTestWebApplicationFactory(postgreSqlFixture);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();

        await CleanupTestDataAsync(_factory.GetConnectionString());

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbSeeder = scope.ServiceProvider.GetRequiredService<LastMile.TMS.Application.Common.Interfaces.IDbSeeder>();
        await dbSeeder.SeedAsync();

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

        // Create a depot and zone for bin tests
        var depotMutation = @"mutation {
            createDepot(input: {
                name: ""Bin Test Depot"",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }
            }) { id }
        }";
        var depotJson = await GraphQLRequestAsync(depotMutation);
        _depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var zoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""B"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{_depotId}""
            }}) {{ id }}
        }}";
        var zoneJson = await GraphQLRequestAsync(zoneMutation);
        _zoneId = zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("id").GetString();
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
    public async Task CreateBin_WithValidInput_ReturnsBin()
    {
        // Arrange
        var mutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}"",
                isActive: true
            }}) {{
                id
                label
                aisle
                slot
                capacity
                isActive
                zoneId
                createdAt
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.GetProperty("data").GetProperty("createBin").TryGetProperty("id", out var id).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("label").GetString().Should().Be("DB-B-A1-01");
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("aisle").GetInt32().Should().Be(1);
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("slot").GetInt32().Should().Be(1);
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("capacity").GetInt32().Should().Be(50);
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("isActive").GetBoolean().Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("zoneId").GetString().Should().Be(_zoneId);
    }

    [Fact]
    public async Task CreateBin_LabelFormat_IsDepotZoneAisleSlot()
    {
        // Arrange - Zone name is "B", Depot name is "Bin Test Depot"
        var mutation = $@"mutation {{
            createBin(input: {{
                aisle: 3,
                slot: 2,
                capacity: 25,
                zoneId: ""{_zoneId}""
            }}) {{
                id
                label
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert - Label should be D{BinTestDepot}-{B}-A3-02
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("label").GetString()
            .Should().StartWith("D");
        jsonResponse.GetProperty("data").GetProperty("createBin").GetProperty("label").GetString()
            .Should().EndWith("-A3-02");
    }

    [Fact]
    public async Task CreateBin_WithInvalidZone_ReturnsError()
    {
        // Arrange
        var fakeZoneId = Guid.NewGuid().ToString();
        var mutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{fakeZoneId}""
            }}) {{
                id
                label
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateBin_AisleZero_ReturnsValidationError()
    {
        // Arrange
        var mutation = $@"mutation {{
            createBin(input: {{
                aisle: 0,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{
                id
                label
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateBin_ValidInput_ReturnsUpdatedBin()
    {
        // Arrange - Create a bin first
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{
                id
                label
            }}
        }}";

        var createJson = await GraphQLRequestAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        // Act - Update the bin
        var updateMutation = $@"mutation {{
            updateBin(input: {{
                id: ""{binId}"",
                aisle: 2,
                slot: 5,
                capacity: 100,
                zoneId: ""{_zoneId}"",
                isActive: false
            }}) {{
                id
                label
                aisle
                slot
                capacity
                isActive
            }}
        }}";

        var updateJson = await GraphQLRequestAsync(updateMutation);

        // Assert - Label changes when aisle/slot change
        updateJson.GetProperty("data").GetProperty("updateBin").GetProperty("aisle").GetInt32().Should().Be(2);
        updateJson.GetProperty("data").GetProperty("updateBin").GetProperty("slot").GetInt32().Should().Be(5);
        updateJson.GetProperty("data").GetProperty("updateBin").GetProperty("capacity").GetInt32().Should().Be(100);
        updateJson.GetProperty("data").GetProperty("updateBin").GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBin_ExistingBin_ReturnsTrue()
    {
        // Arrange - Create a bin first
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{ id }}
        }}";

        var createJson = await GraphQLRequestAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        // Act
        var deleteMutation = $@"mutation {{
            deleteBin(id: ""{binId}"")
        }}";

        var deleteJson = await GraphQLRequestAsync(deleteMutation);

        // Assert
        deleteJson.GetProperty("data").GetProperty("deleteBin").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBin_NonExistingBin_ReturnsFalse()
    {
        // Arrange
        var fakeBinId = Guid.NewGuid().ToString();

        // Act
        var deleteMutation = $@"mutation {{
            deleteBin(id: ""{fakeBinId}"")
        }}";

        var deleteJson = await GraphQLRequestAsync(deleteMutation);

        // Assert
        deleteJson.GetProperty("data").GetProperty("deleteBin").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task QueryBin_ById_ReturnsBin()
    {
        // Arrange - Create a bin
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 5,
                slot: 10,
                capacity: 75,
                zoneId: ""{_zoneId}""
            }}) {{ id }}
        }}";

        var createJson = await GraphQLRequestAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        // Act
        var query = $@"query {{
            bin(id: ""{binId}"") {{
                id
                label
                aisle
                slot
                capacity
                isActive
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        // Assert
        queryJson.GetProperty("data").GetProperty("bin").GetProperty("label").GetString().Should().Be("DB-B-A5-10");
        queryJson.GetProperty("data").GetProperty("bin").GetProperty("aisle").GetInt32().Should().Be(5);
        queryJson.GetProperty("data").GetProperty("bin").GetProperty("slot").GetInt32().Should().Be(10);
        queryJson.GetProperty("data").GetProperty("bin").GetProperty("capacity").GetInt32().Should().Be(75);
    }

    [Fact]
    public async Task QueryBins_ReturnsAllBins()
    {
        // Arrange - Create two bins
        await GraphQLRequestAsync($@"mutation {{
            createBin(input: {{ aisle: 1, slot: 1, capacity: 50, zoneId: ""{_zoneId}"" }}) {{ id }}
        }}");
        await GraphQLRequestAsync($@"mutation {{
            createBin(input: {{ aisle: 1, slot: 2, capacity: 60, zoneId: ""{_zoneId}"" }}) {{ id }}
        }}");

        // Act
        var query = @"query {
            bins {
                nodes {
                    id
                    label
                }
            }
        }";

        var queryJson = await GraphQLRequestAsync(query);

        // Assert
        var bins = queryJson.GetProperty("data").GetProperty("bins").GetProperty("nodes");
        bins.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task QueryBinUtilizations_ReturnsUtilizationData()
    {
        // Arrange - Create a bin
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{ id }}
        }}";

        await GraphQLRequestAsync(createMutation);

        // Act
        var query = $@"query {{
            binUtilizations(zoneId: ""{_zoneId}"") {{
                id
                label
                aisle
                slot
                capacity
                utilizationPercent
                isActive
            }}
        }}";

        var queryJson = await GraphQLRequestAsync(query);

        // Assert
        var utilizations = queryJson.GetProperty("data").GetProperty("binUtilizations");
        utilizations.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);

        var bin = utilizations.EnumerateArray().First(b =>
            b.GetProperty("aisle").GetInt32() == 1 &&
            b.GetProperty("slot").GetInt32() == 1);

        bin.GetProperty("label").GetString().Should().Be("DB-B-A1-01");
        bin.GetProperty("capacity").GetInt32().Should().Be(50);
        bin.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    private static async Task CleanupTestDataAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Bins\";", connection))
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

    [Fact]
    public async Task GetBinPdfLabel_ReturnsPdfFile()
    {
        // Arrange - Create a bin first
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{
                id
            }}
        }}";
        var createJson = await GraphQLRequestAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        // Act
        var pdfResponse = await _client.GetAsync($"/labels/bin/{binId}/pdf");

        // Assert - Check status and content type
        pdfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pdfResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();

        // Assert - Verify it's a valid PDF
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Take(4).Should().Contain(new byte[] { 0x25, 0x50, 0x46, 0x25 }); // PDF magic number
    }

    [Fact]
    public async Task GetBinPngLabel_ReturnsPngFile()
    {
        // Arrange - Create a bin first
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{
                id
            }}
        }}";
        var createJson = await GraphQLRequestAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        // Act
        var pngResponse = await _client.GetAsync($"/labels/bin/{binId}/png");

        // Assert - Check status and content type
        pngResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pngResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/png");

        var pngBytes = await pngResponse.Content.ReadAsByteArrayAsync();

        // Assert - Verify it's a valid PNG image
        pngBytes.Should().NotBeEmpty();
        pngBytes.Take(8).Should().Contain(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG magic number
    }

    [Fact]
    public async Task GetBinZplLabel_InvalidBinId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/labels/bin/{Guid.NewGuid()}/zpl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBinPdfLabel_InvalidBinId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/labels/bin/{Guid.NewGuid()}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBinPngLabel_InvalidBinId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/labels/bin/{Guid.NewGuid()}/png");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBinZplLabel_ReturnsZplText()
    {
        // Arrange - Create a bin first
        var createMutation = $@"mutation {{
            createBin(input: {{
                aisle: 1,
                slot: 1,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{
                id
            }}
        }}";
        var createJson = await GraphQLRequestAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/labels/bin/{binId}/zpl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBin_DuplicateLabel_ReturnsError()
    {
        // Arrange - Create first bin
        var createMutation1 = $@"mutation {{
            createBin(input: {{
                aisle: 10,
                slot: 10,
                capacity: 50,
                zoneId: ""{_zoneId}""
            }}) {{
                id
                label
            }}
        }}";
        var firstJson = await GraphQLRequestAsync(createMutation1);
        firstJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString().Should().NotBeNull();

        // Act - Attempt to create second bin with same aisle/slot in same zone (duplicate label)
        var createMutation2 = $@"mutation {{
            createBin(input: {{
                aisle: 10,
                slot: 10,
                capacity: 100,
                zoneId: ""{_zoneId}""
            }}) {{
                id
            }}
        }}";
        
        // Assert - GraphQL should return errors
        var secondResponse = await _client.PostAsync("/api/graphql", new StringContent(
            System.Text.Json.JsonSerializer.Serialize(new { query = createMutation2 }),
            System.Text.Encoding.UTF8,
            "application/json"));

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK); // GraphQL usually returns 200 with errors array
        var secondJsonString = await secondResponse.Content.ReadAsStringAsync();
        var secondJson = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(secondJsonString);
        
        secondJson.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("already exists");
    }
}
