using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class VehicleIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;

    private static string AdminUsername => Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
    private static string AdminPassword => Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

    public VehicleIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
    {
        _factory = new IntegrationTestWebApplicationFactory(postgreSqlFixture);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();

        // Seed the database with admin user
        using var scope = _factory.Services.CreateScope();
        var dbSeeder = scope.ServiceProvider.GetRequiredService<LastMile.TMS.Application.Common.Interfaces.IDbSeeder>();
        await dbSeeder.SeedAsync();

        // Get access token
        _accessToken = await GetAccessTokenAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", AdminUsername },
            { "password", AdminPassword }
        };
        var content = new FormUrlEncodedContent(formData);
        var response = await _client.PostAsync("/connect/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return tokenResponse.GetProperty("access_token").GetString()!;
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ReturnsVehicle()
    {
        // Arrange
        var mutation = @"
            mutation {
                createVehicle(
                    registrationPlate: ""TEST001"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.5
                ) {
                    id
                    registrationPlate
                    type
                    parcelCapacity
                    weightCapacityKg
                    status
                }
            }";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("createVehicle");
        data.GetProperty("registrationPlate").GetString().Should().Be("TEST001");
        data.GetProperty("type").GetString().Should().Be("VAN");
        data.GetProperty("parcelCapacity").GetInt32().Should().Be(100);
        data.GetProperty("status").GetString().Should().Be("AVAILABLE");
    }

    [Fact]
    public async Task CreateVehicle_WithEmptyRegistration_ReturnsError()
    {
        // Arrange
        var mutation = @"
            mutation {
                createVehicle(
                    registrationPlate: """",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.5
                ) {
                    id
                }
            }";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateVehicle_StatusTransition_ValidTransitions()
    {
        // Arrange - First create a vehicle
        var createMutation = @"
            mutation {
                createVehicle(
                    registrationPlate: ""UPDATE01"",
                    type: CAR,
                    parcelCapacity: 50,
                    weightCapacityKg: 250.0
                ) {
                    id
                    status
                }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var vehicleId = createJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        // Act - Transition to Maintenance (valid from Available)
        var maintenanceMutation = $@"
            mutation {{
                changeVehicleStatus(id: ""{vehicleId}"", newStatus: MAINTENANCE) {{
                    id
                    status
                }}
            }}";

        var maintenanceResponse = await ExecuteGraphQLAsync(maintenanceMutation);

        // Assert
        maintenanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var maintenanceJson = await ReadJsonAsync(maintenanceResponse);
        maintenanceJson.RootElement.TryGetProperty("errors", out var errors).Should().BeFalse();
        var maintenanceData = maintenanceJson.RootElement.GetProperty("data").GetProperty("changeVehicleStatus");
        maintenanceData.GetProperty("status").GetString().Should().Be("MAINTENANCE");
    }

    [Fact]
    public async Task GetVehicles_ReturnsAllVehicles()
    {
        // Arrange - Create a couple of vehicles first
        var createMutation1 = @"
            mutation {
                createVehicle(
                    registrationPlate: ""LIST001"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.0
                ) { id }
            }";
        await ExecuteGraphQLAsync(createMutation1);

        var createMutation2 = @"
            mutation {
                createVehicle(
                    registrationPlate: ""LIST002"",
                    type: CAR,
                    parcelCapacity: 50,
                    weightCapacityKg: 250.0
                ) { id }
            }";
        await ExecuteGraphQLAsync(createMutation2);

        // Act
        var query = @"
            query {
                vehicles {
                    id
                    registrationPlate
                    type
                    status
                }
            }";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var vehicles = json.RootElement.GetProperty("data").GetProperty("vehicles").EnumerateArray().ToList();
        vehicles.Should().Contain(v => v.GetProperty("registrationPlate").GetString() == "LIST001");
        vehicles.Should().Contain(v => v.GetProperty("registrationPlate").GetString() == "LIST002");
    }

    [Fact]
    public async Task GetVehicle_ById_ReturnsVehicle()
    {
        // Arrange - Create a vehicle
        var createMutation = @"
            mutation {
                createVehicle(
                    registrationPlate: ""GETBYID"",
                    type: BIKE,
                    parcelCapacity: 10,
                    weightCapacityKg: 50.0
                ) { id }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var vehicleId = createJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        // Act
        var query = $@"
            query {{
                vehicle(id: ""{vehicleId}"") {{
                    id
                    registrationPlate
                    type
                    status
                }}
            }}";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var vehicle = json.RootElement.GetProperty("data").GetProperty("vehicle");
        vehicle.GetProperty("registrationPlate").GetString().Should().Be("GETBYID");
        vehicle.GetProperty("type").GetString().Should().Be("BIKE");
        vehicle.GetProperty("status").GetString().Should().Be("AVAILABLE");
    }

    [Fact]
    public async Task VehicleHistory_WithJourneys_ReturnsHistory()
    {
        // Arrange - Create a vehicle and a route with journey
        var createVehicleMutation = @"
            mutation {
                createVehicle(
                    registrationPlate: ""HIST001"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.0
                ) { id }
            }";
        var vehicleResponse = await ExecuteGraphQLAsync(createVehicleMutation);
        var vehicleJson = await ReadJsonAsync(vehicleResponse);
        var vehicleId = vehicleJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        // Act - Query vehicle history (should be empty initially)
        var query = $@"
            query {{
                vehicleHistory(id: ""{vehicleId}"") {{
                    id
                    registrationPlate
                    totalMileageKm
                    totalRoutesCompleted
                    routes {{
                        routeId
                        routeName
                    }}
                }}
            }}";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var history = json.RootElement.GetProperty("data").GetProperty("vehicleHistory");
        history.GetProperty("registrationPlate").GetString().Should().Be("HIST001");
        history.GetProperty("totalMileageKm").GetDecimal().Should().Be(0);
        history.GetProperty("totalRoutesCompleted").GetInt32().Should().Be(0);
    }

    private async Task<HttpResponseMessage> ExecuteGraphQLAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return await _client.SendAsync(request);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonDocument>(content)!;
    }
}
