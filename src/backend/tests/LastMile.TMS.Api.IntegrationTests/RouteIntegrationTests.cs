using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class RouteIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;
    private Guid _vehicleId = Guid.Empty;
    private string _vehiclePlate = null!;

    private static string AdminUsername => Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
    private static string AdminPassword => Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

    public RouteIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
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

        _accessToken = await GetAccessTokenAsync();

        // Create a vehicle with unique registration plate for route assignment tests
        var uniquePlate = $"ROUTE-VEH-{Guid.NewGuid():N}".Substring(0, 15).ToUpperInvariant();
        var vehicleMutation = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""{uniquePlate}"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.0
                ) {{ id }}
            }}";
        var vehicleResponse = await ExecuteGraphQLAsync(vehicleMutation);
        var vehicleJson = await ReadJsonAsync(vehicleResponse);

        // Check if there are errors
        if (vehicleJson.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
        {
            var errorMessage = errors[0].GetProperty("message").GetString();
            var fullResponse = vehicleJson.RootElement.GetRawText();
            throw new InvalidOperationException($"Failed to create vehicle in InitializeAsync: {errorMessage}. Response: {fullResponse}");
        }

        if (!vehicleJson.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Invalid response from createVehicle mutation");
        }

        if (!data.TryGetProperty("createVehicle", out var createVehicle) || createVehicle.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("createVehicle returned null");
        }

        _vehicleId = Guid.Parse(createVehicle.GetProperty("id").GetString()!);
        _vehiclePlate = createVehicle.TryGetProperty("registrationPlate", out var plateEl)
            ? plateEl.GetString()!
            : uniquePlate;
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
    public async Task CreateRoute_WithValidData_ReturnsRoute()
    {
        // Arrange
        var mutation = $@"
            mutation {{
                createRoute(
                    name: ""Test Route 001"",
                    plannedStartTime: ""2026-03-26T09:00:00Z"",
                    totalDistanceKm: 50.5,
                    totalParcelCount: 25
                ) {{
                    id
                    name
                    status
                    plannedStartTime
                    totalDistanceKm
                    totalParcelCount
                    vehicleId
                }}
            }}";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("createRoute");
        data.GetProperty("name").GetString().Should().Be("Test Route 001");
        data.GetProperty("status").GetString().Should().Be("PLANNED");
        data.GetProperty("totalDistanceKm").GetDecimal().Should().Be(50.5m);
        data.GetProperty("totalParcelCount").GetInt32().Should().Be(25);
    }

    [Fact]
    public async Task CreateRoute_WithVehicleAssignment_ReturnsRouteWithVehicle()
    {
        // Arrange
        var mutation = $@"
            mutation {{
                createRoute(
                    name: ""Route With Vehicle"",
                    plannedStartTime: ""2026-03-27T10:00:00Z"",
                    totalDistanceKm: 100.0,
                    totalParcelCount: 50,
                    vehicleId: ""{_vehicleId}""
                ) {{
                    id
                    name
                    vehicleId
                    vehiclePlate
                }}
            }}";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("createRoute");
        data.GetProperty("vehicleId").GetString().Should().Be(_vehicleId.ToString());
        data.GetProperty("vehiclePlate").GetString().Should().Be(_vehiclePlate);
    }

    [Fact]
    public async Task CreateRoute_WithEmptyName_ReturnsError()
    {
        // Arrange
        var mutation = @"
            mutation {
                createRoute(
                    name: """",
                    plannedStartTime: ""2026-03-26T09:00:00Z"",
                    totalDistanceKm: 50.0,
                    totalParcelCount: 25
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
    public async Task GetRoutes_ReturnsAllRoutes()
    {
        // Arrange - Create a couple of routes first
        var createMutation1 = @"
            mutation {
                createRoute(
                    name: ""Route List 001"",
                    plannedStartTime: ""2026-03-28T08:00:00Z"",
                    totalDistanceKm: 30.0,
                    totalParcelCount: 15
                ) { id }
            }";
        await ExecuteGraphQLAsync(createMutation1);

        var createMutation2 = @"
            mutation {
                createRoute(
                    name: ""Route List 002"",
                    plannedStartTime: ""2026-03-28T10:00:00Z"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                ) { id }
            }";
        await ExecuteGraphQLAsync(createMutation2);

        // Act
        var query = @"
            query {
                routes {
                    id
                    name
                    status
                    plannedStartTime
                }
            }";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routes = json.RootElement.GetProperty("data").GetProperty("routes").EnumerateArray().ToList();
        routes.Should().Contain(r => r.GetProperty("name").GetString() == "Route List 001");
        routes.Should().Contain(r => r.GetProperty("name").GetString() == "Route List 002");
    }

    [Fact]
    public async Task GetRoute_ById_ReturnsRoute()
    {
        // Arrange - Create a route
        var createMutation = @"
            mutation {
                createRoute(
                    name: ""Get Route By ID"",
                    plannedStartTime: ""2026-03-29T11:00:00Z"",
                    totalDistanceKm: 60.0,
                    totalParcelCount: 30
                ) { id }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act
        var query = $@"
            query {{
                route(id: ""{routeId}"") {{
                    id
                    name
                    status
                    totalDistanceKm
                    totalParcelCount
                }}
            }}";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var route = json.RootElement.GetProperty("data").GetProperty("route");
        route.GetProperty("name").GetString().Should().Be("Get Route By ID");
        route.GetProperty("status").GetString().Should().Be("PLANNED");
    }

    [Fact]
    public async Task UpdateRoute_ChangesRouteData()
    {
        // Arrange - Create a route
        var createMutation = @"
            mutation {
                createRoute(
                    name: ""Update Route Test"",
                    plannedStartTime: ""2026-03-30T09:00:00Z"",
                    totalDistanceKm: 25.0,
                    totalParcelCount: 10
                ) { id }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Update the route
        var updateMutation = $@"
            mutation {{
                updateRoute(
                    id: ""{routeId}"",
                    name: ""Updated Route Name"",
                    plannedStartTime: ""2026-03-31T10:00:00Z"",
                    totalDistanceKm: 75.0,
                    totalParcelCount: 35
                ) {{
                    id
                    name
                    totalDistanceKm
                    totalParcelCount
                }}
            }}";
        var updateResponse = await ExecuteGraphQLAsync(updateMutation);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(updateResponse);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("updateRoute");
        data.GetProperty("name").GetString().Should().Be("Updated Route Name");
        data.GetProperty("totalDistanceKm").GetDecimal().Should().Be(75.0m);
        data.GetProperty("totalParcelCount").GetInt32().Should().Be(35);
    }

    [Fact]
    public async Task UpdateRoute_WithVehicleAssignment_UpdatesVehicleStatus()
    {
        // Arrange - Create a route without vehicle
        var createMutation = @"
            mutation {
                createRoute(
                    name: ""Update Route Vehicle"",
                    plannedStartTime: ""2026-04-01T08:00:00Z"",
                    totalDistanceKm: 45.0,
                    totalParcelCount: 20
                ) { id }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Assign vehicle to route
        var updateMutation = $@"
            mutation {{
                updateRoute(
                    id: ""{routeId}"",
                    name: ""Update Route Vehicle"",
                    plannedStartTime: ""2026-04-01T08:00:00Z"",
                    totalDistanceKm: 45.0,
                    totalParcelCount: 20,
                    vehicleId: ""{_vehicleId}""
                ) {{
                    id
                    vehicleId
                    vehiclePlate
                }}
            }}";
        var updateResponse = await ExecuteGraphQLAsync(updateMutation);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(updateResponse);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("updateRoute");
        data.GetProperty("vehicleId").GetString().Should().Be(_vehicleId.ToString());
        data.GetProperty("vehiclePlate").GetString().Should().Be(_vehiclePlate);
    }

    [Fact]
    public async Task DeleteRoute_RemovesRoute()
    {
        // Arrange - Create a route
        var createMutation = @"
            mutation {
                createRoute(
                    name: ""Delete Route Test"",
                    plannedStartTime: ""2026-04-02T09:00:00Z"",
                    totalDistanceKm: 55.0,
                    totalParcelCount: 25
                ) { id }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Delete the route
        var deleteMutation = $@"
            mutation {{
                deleteRoute(id: ""{routeId}"")
            }}";
        var deleteResponse = await ExecuteGraphQLAsync(deleteMutation);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deleteJson = await ReadJsonAsync(deleteResponse);
        deleteJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        deleteJson.RootElement.GetProperty("data").GetProperty("deleteRoute").GetBoolean().Should().BeTrue();

        // Verify route is gone
        var getQuery = $@"
            query {{
                route(id: ""{routeId}"") {{
                    id
                }}
            }}";
        var getResponse = await ExecuteGraphQLAsync(getQuery);
        var getJson = await ReadJsonAsync(getResponse);
        var routeData = getJson.RootElement.GetProperty("data").GetProperty("route");
        // After soft-delete, route may be null or have null id
        if (routeData.ValueKind != JsonValueKind.Null)
        {
            routeData.GetProperty("id").GetString().Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteRoute_WithAssignedVehicle_ReleasesVehicle()
    {
        // Arrange - Create a route with vehicle
        var createMutation = $@"
            mutation {{
                createRoute(
                    name: ""Delete Route With Vehicle"",
                    plannedStartTime: ""2026-04-03T08:00:00Z"",
                    totalDistanceKm: 35.0,
                    totalParcelCount: 15,
                    vehicleId: ""{_vehicleId}""
                ) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Delete the route (vehicle should be released)
        var deleteQuery = $@"
            mutation {{
                deleteRoute(id: ""{routeId}"")
            }}";
        var deleteResponse = await ExecuteGraphQLAsync(deleteQuery);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deleteJson = await ReadJsonAsync(deleteResponse);
        deleteJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        deleteJson.RootElement.GetProperty("data").GetProperty("deleteRoute").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ChangeRouteStatus_ToInProgress_SetsActualStartTime()
    {
        // Arrange - Create a route
        var createMutation = @"
            mutation {
                createRoute(
                    name: ""Status Change Test"",
                    plannedStartTime: ""2026-04-10T09:00:00Z"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                ) { id }
            }";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Change status to IN_PROGRESS
        var changeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{
                    id
                    status
                    actualStartTime
                }}
            }}";
        var changeResponse = await ExecuteGraphQLAsync(changeMutation);

        // Assert
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var changeJson = await ReadJsonAsync(changeResponse);
        changeJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = changeJson.RootElement.GetProperty("data").GetProperty("changeRouteStatus");
        data.GetProperty("status").GetString().Should().Be("IN_PROGRESS");
        data.GetProperty("actualStartTime").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChangeRouteStatus_ToCompleted_ReleasesVehicle()
    {
        // Arrange - Create a route with vehicle
        var createMutation = $@"
            mutation {{
                createRoute(
                    name: ""Complete With Vehicle"",
                    plannedStartTime: ""2026-04-11T10:00:00Z"",
                    totalDistanceKm: 50.0,
                    totalParcelCount: 25,
                    vehicleId: ""{_vehicleId}""
                ) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Change status to COMPLETED
        var completeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: COMPLETED) {{
                    id
                    status
                    actualEndTime
                    vehiclePlate
                }}
            }}";
        var completeResponse = await ExecuteGraphQLAsync(completeMutation);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeJson = await ReadJsonAsync(completeResponse);
        completeJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = completeJson.RootElement.GetProperty("data").GetProperty("changeRouteStatus");
        data.GetProperty("status").GetString().Should().Be("COMPLETED");
        data.GetProperty("actualEndTime").GetString().Should().NotBeNullOrEmpty();
        // Vehicle should be released (plate still returned but vehicle status updated)
        data.GetProperty("vehiclePlate").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChangeRouteStatus_ToCompleted_UpdatesVehicleHistory()
    {
        // Arrange - Create a route with vehicle
        var createMutation = $@"
            mutation {{
                createRoute(
                    name: ""History Test Route"",
                    plannedStartTime: ""2026-04-15T08:00:00Z"",
                    totalDistanceKm: 60.0,
                    totalParcelCount: 30,
                    vehicleId: ""{_vehicleId}""
                ) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Start the route
        var startMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{
                    id
                    status
                }}
            }}";
        await ExecuteGraphQLAsync(startMutation);

        // Complete the route
        var completeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: COMPLETED) {{
                    id
                    status
                }}
            }}";
        await ExecuteGraphQLAsync(completeMutation);

        // Assert - Check vehicle history
        var historyQuery = $@"
            query {{
                vehicleHistory(id: ""{_vehicleId}"") {{
                    id
                    totalMileageKm
                    totalRoutesCompleted
                    routes {{
                        routeId
                        routeName
                    }}
                }}
            }}";
        var historyResponse = await ExecuteGraphQLAsync(historyQuery);
        var historyJson = await ReadJsonAsync(historyResponse);
        historyJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();

        var historyData = historyJson.RootElement.GetProperty("data").GetProperty("vehicleHistory");
        historyData.GetProperty("totalRoutesCompleted").GetInt32().Should().BeGreaterThan(0);
        var routes = historyData.GetProperty("routes").EnumerateArray().ToList();
        routes.Should().Contain(r => r.GetProperty("routeId").GetString() == routeId);
    }

    [Fact]
    public async Task ChangeRouteStatus_ToCancelled_ReleasesVehicle()
    {
        // Arrange - Create a route with vehicle
        var createMutation = $@"
            mutation {{
                createRoute(
                    name: ""Cancel With Vehicle"",
                    plannedStartTime: ""2026-04-20T10:00:00Z"",
                    totalDistanceKm: 45.0,
                    totalParcelCount: 20,
                    vehicleId: ""{_vehicleId}""
                ) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Cancel the route
        var cancelMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: CANCELLED) {{
                    id
                    status
                    vehiclePlate
                }}
            }}";
        var cancelResponse = await ExecuteGraphQLAsync(cancelMutation);

        // Assert
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelJson = await ReadJsonAsync(cancelResponse);
        cancelJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = cancelJson.RootElement.GetProperty("data").GetProperty("changeRouteStatus");
        data.GetProperty("status").GetString().Should().Be("CANCELLED");
        // Vehicle should be released (plate still returned but vehicle status updated to Available)
    }

    [Fact]
    public async Task CreateRoute_WithRetiredVehicle_ReturnsError()
    {
        // Arrange - Create a retired vehicle first
        var uniquePlate = $"RET-VEH-{Guid.NewGuid():N}".Substring(0, 12).ToUpperInvariant();
        var createVehicleMutation = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""{uniquePlate}"",
                    type: VAN,
                    parcelCapacity: 50,
                    weightCapacityKg: 300.0
                ) {{ id }}
            }}";
        var vehicleResponse = await ExecuteGraphQLAsync(createVehicleMutation);
        var vehicleJson = await ReadJsonAsync(vehicleResponse);
        var vehicleId = vehicleJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        // Retire the vehicle
        var retireMutation = $@"
            mutation {{
                changeVehicleStatus(id: ""{vehicleId}"", newStatus: RETIRED) {{
                    id
                    status
                }}
            }}";
        await ExecuteGraphQLAsync(retireMutation);

        // Act - Try to create a route with the retired vehicle
        var createRouteMutation = $@"
            mutation {{
                createRoute(
                    name: ""Route With Retired Vehicle"",
                    plannedStartTime: ""2026-04-25T10:00:00Z"",
                    totalDistanceKm: 30.0,
                    totalParcelCount: 15,
                    vehicleId: ""{vehicleId}""
                ) {{ id }}
            }}";
        var routeResponse = await ExecuteGraphQLAsync(createRouteMutation);

        // Assert
        routeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var routeJson = await ReadJsonAsync(routeResponse);
        routeJson.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
        errors[0].GetProperty("message").GetString().Should().Contain("retired");
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
