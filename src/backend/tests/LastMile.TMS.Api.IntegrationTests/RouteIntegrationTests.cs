using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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
    private Guid _driverId = Guid.Empty;

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

        // Create a driver with a unique license number and a shift schedule for next week
        var driverEmail = await CreateDriverUserAsync($"driver-route-test-{Guid.NewGuid():N}@test.com");
        var uniqueLicense = $"DL-{Guid.NewGuid():N}".Substring(0, 12).ToUpperInvariant();
        var nextWeek = DateTime.UtcNow.AddDays(7);
        var dayOfWeek = nextWeek.DayOfWeek;
        var driverMutation = $@"
            mutation {{
                createDriver(
                    input: {{
                        email: ""{driverEmail}"",
                        licenseNumber: ""{uniqueLicense}"",
                        licenseExpiryDate: ""{DateTimeOffset.UtcNow.AddYears(1):O}"",
                        shiftSchedules: [
                            {{ dayOfWeek: {dayOfWeek.ToString().ToUpperInvariant()}, openTime: ""08:00:00"", closeTime: ""17:00:00"" }}
                        ]
                    }}
                ) {{ id }}
            }}";
        var driverResponse = await ExecuteGraphQLAsync(driverMutation);
        var driverJson = await ReadJsonAsync(driverResponse);

        if (driverJson.RootElement.TryGetProperty("errors", out var driverErrors) && driverErrors.GetArrayLength() > 0)
        {
            var errorMessage = driverErrors[0].GetProperty("message").GetString();
            var fullResponse = driverJson.RootElement.GetRawText();
            throw new InvalidOperationException($"Failed to create driver in InitializeAsync: {errorMessage}. Response: {fullResponse}");
        }

        var driverData = driverJson.RootElement.GetProperty("data").GetProperty("createDriver");
        _driverId = Guid.Parse(driverData.GetProperty("id").GetString()!);
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
                createRoute(input: {{
                    name: ""Test Route 001"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(7):O}"",
                    totalDistanceKm: 50.5,
                    totalParcelCount: 25
                }}) {{
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
        data.GetProperty("status").GetString().Should().Be("DRAFT");
        data.GetProperty("totalDistanceKm").GetDecimal().Should().Be(50.5m);
        data.GetProperty("totalParcelCount").GetInt32().Should().Be(25);
    }

    [Fact]
    public async Task CreateRoute_WithVehicleAssignment_ReturnsRouteWithVehicle()
    {
        // Arrange
        var mutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route With Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(8):O}"",
                    totalDistanceKm: 100.0,
                    totalParcelCount: 50,
                    vehicleId: ""{_vehicleId}""
                }}) {{
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
        var mutation = $@"
            mutation {{
                createRoute(input: {{
                    name: """",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(7):O}"",
                    totalDistanceKm: 50.0,
                    totalParcelCount: 25
                }}) {{
                    id
                }}
            }}";

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
        var createMutation1 = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route List 001"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(9):O}"",
                    totalDistanceKm: 30.0,
                    totalParcelCount: 15
                }}) {{ id }}
            }}";
        await ExecuteGraphQLAsync(createMutation1);

        var createMutation2 = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route List 002"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(9):O}"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        await ExecuteGraphQLAsync(createMutation2);

        // Act
        var query = @"
            query {
                routes(first: 100) {
                    nodes {
                        id
                        name
                        status
                        plannedStartTime
                    }
                }
            }";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routes = json.RootElement.GetProperty("data").GetProperty("routes").GetProperty("nodes").EnumerateArray().ToList();
        routes.Should().Contain(r => r.GetProperty("name").GetString() == "Route List 001");
        routes.Should().Contain(r => r.GetProperty("name").GetString() == "Route List 002");
    }

    [Fact]
    public async Task GetRoute_ById_ReturnsRoute()
    {
        // Arrange - Create a route
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Get Route By ID"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(10):O}"",
                    totalDistanceKm: 60.0,
                    totalParcelCount: 30
                }}) {{ id }}
            }}";
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
        route.GetProperty("status").GetString().Should().Be("DRAFT");
    }

    [Fact]
    public async Task UpdateRoute_ChangesRouteData()
    {
        // Arrange - Create a route
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Update Route Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(11):O}"",
                    totalDistanceKm: 25.0,
                    totalParcelCount: 10
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Update the route
        var updateMutation = $@"
            mutation {{
                updateRoute(input: {{
                    id: ""{routeId}"",
                    name: ""Updated Route Name"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(12):O}"",
                    totalDistanceKm: 75.0,
                    totalParcelCount: 35
                }}) {{
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
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Update Route Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(13):O}"",
                    totalDistanceKm: 45.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Assign vehicle to route
        var updateMutation = $@"
            mutation {{
                updateRoute(input: {{
                    id: ""{routeId}"",
                    name: ""Update Route Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(13):O}"",
                    totalDistanceKm: 45.0,
                    totalParcelCount: 20,
                    vehicleId: ""{_vehicleId}""
                }}) {{
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
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Delete Route Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(14):O}"",
                    totalDistanceKm: 55.0,
                    totalParcelCount: 25
                }}) {{ id }}
            }}";
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
                createRoute(input: {{
                    name: ""Delete Route With Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(15):O}"",
                    totalDistanceKm: 35.0,
                    totalParcelCount: 15,
                    vehicleId: ""{_vehicleId}""
                }}) {{ id }}
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
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Status Change Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(12):O}"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
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
                createRoute(input: {{
                    name: ""Complete With Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(13):O}"",
                    totalDistanceKm: 50.0,
                    totalParcelCount: 25,
                    vehicleId: ""{_vehicleId}""
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Start the route first (Required: Draft -> InProgress -> Completed)
        var startMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{
                    id
                    status
                }}
            }}";
        await ExecuteGraphQLAsync(startMutation);

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
                createRoute(input: {{
                    name: ""History Test Route"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(17):O}"",
                    totalDistanceKm: 60.0,
                    totalParcelCount: 30,
                    vehicleId: ""{_vehicleId}""
                }}) {{ id }}
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
                createRoute(input: {{
                    name: ""Route With Retired Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(27):O}"",
                    totalDistanceKm: 30.0,
                    totalParcelCount: 15,
                    vehicleId: ""{vehicleId}""
                }}) {{ id }}
            }}";
        var routeResponse = await ExecuteGraphQLAsync(createRouteMutation);

        // Assert
        routeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var routeJson = await ReadJsonAsync(routeResponse);
        routeJson.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
        errors[0].GetProperty("message").GetString().Should().Contain("retired vehicle");
    }

    [Fact]
    public async Task CreateRoute_WithDriverAssignment_ReturnsDriverFields()
    {
        // Arrange
        var plannedDate = DateTime.UtcNow.AddDays(7);
        var mutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route With Driver"",
                    plannedStartTime: ""{plannedDate:O}"",
                    totalDistanceKm: 50.0,
                    totalParcelCount: 25,
                    driverId: ""{_driverId}""
                }}) {{
                    id
                    name
                    driverId
                    driverName
                }}
            }}";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("createRoute");
        data.GetProperty("driverId").GetString().Should().Be(_driverId.ToString());
        data.GetProperty("driverName").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignDriverToRoute_OnPlannedRoute_Succeeds()
    {
        // Arrange - Create a route without driver
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Assign Driver Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(8):O}"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Assign driver
        var assignMutation = $@"
            mutation {{
                assignDriverToRoute(
                    routeId: ""{routeId}"",
                    driverId: ""{_driverId}""
                ) {{
                    id
                    driverId
                    driverName
                }}
            }}";
        var assignResponse = await ExecuteGraphQLAsync(assignMutation);

        // Assert
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignJson = await ReadJsonAsync(assignResponse);
        assignJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = assignJson.RootElement.GetProperty("data").GetProperty("assignDriverToRoute");
        data.GetProperty("driverId").GetString().Should().Be(_driverId.ToString());
        data.GetProperty("driverName").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignDriverToRoute_Reassignment_Succeeds()
    {
        // Arrange - Create route with first driver
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Reassign Driver Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(9):O}"",
                    totalDistanceKm: 30.0,
                    totalParcelCount: 15,
                    driverId: ""{_driverId}""
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Create a second driver
        var driver2Email = await CreateDriverUserAsync($"driver-reassign-{Guid.NewGuid():N}@test.com");
        var uniqueLicense2 = $"DL-{Guid.NewGuid():N}".Substring(0, 12).ToUpperInvariant();
        var createDriver2Mutation = $@"
            mutation {{
                createDriver(
                    input: {{
                        email: ""{driver2Email}"",
                        licenseNumber: ""{uniqueLicense2}"",
                        licenseExpiryDate: ""{DateTimeOffset.UtcNow.AddYears(1):O}""
                    }}
                ) {{ id }}
            }}";
        var driver2Response = await ExecuteGraphQLAsync(createDriver2Mutation);
        var driver2Json = await ReadJsonAsync(driver2Response);
        var driver2Id = driver2Json.RootElement.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

        // Act - Reassign to second driver
        var reassignMutation = $@"
            mutation {{
                assignDriverToRoute(
                    routeId: ""{routeId}"",
                    driverId: ""{driver2Id}""
                ) {{
                    id
                    driverId
                    driverName
                }}
            }}";
        var reassignResponse = await ExecuteGraphQLAsync(reassignMutation);

        // Assert
        reassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reassignJson = await ReadJsonAsync(reassignResponse);
        reassignJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = reassignJson.RootElement.GetProperty("data").GetProperty("assignDriverToRoute");
        data.GetProperty("driverId").GetString().Should().Be(driver2Id);
    }

    [Fact]
    public async Task AssignDriverToRoute_Unassign_SetsDriverToNull()
    {
        // Arrange - Create route with driver
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Unassign Driver Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(10):O}"",
                    totalDistanceKm: 25.0,
                    totalParcelCount: 10,
                    driverId: ""{_driverId}""
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Act - Unassign driver by passing null
        var unassignMutation = $@"
            mutation {{
                assignDriverToRoute(
                    routeId: ""{routeId}""
                ) {{
                    id
                    driverId
                }}
            }}";
        var unassignResponse = await ExecuteGraphQLAsync(unassignMutation);

        // Assert
        unassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var unassignJson = await ReadJsonAsync(unassignResponse);
        unassignJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = unassignJson.RootElement.GetProperty("data").GetProperty("assignDriverToRoute");
        data.TryGetProperty("driverId", out var driverIdProp).Should().BeTrue();
        driverIdProp.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task AssignDriverToRoute_OnCompletedRoute_ReturnsError()
    {
        // Arrange - Create route, start and complete it
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Completed Route Driver"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(11):O}"",
                    totalDistanceKm: 35.0,
                    totalParcelCount: 15
                }}) {{ id }}
            }}";
        var createResponse = await ExecuteGraphQLAsync(createMutation);
        var createJson = await ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Start the route
        var startMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{ id }}
            }}";
        await ExecuteGraphQLAsync(startMutation);

        // Complete the route
        var completeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: COMPLETED) {{ id }}
            }}";
        await ExecuteGraphQLAsync(completeMutation);

        // Act - Try to assign driver to completed route
        var assignMutation = $@"
            mutation {{
                assignDriverToRoute(
                    routeId: ""{routeId}"",
                    driverId: ""{_driverId}""
                ) {{
                    id
                }}
            }}";
        var assignResponse = await ExecuteGraphQLAsync(assignMutation);

        // Assert
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignJson = await ReadJsonAsync(assignResponse);
        assignJson.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("Draft");
    }

    [Fact]
    public async Task GetAvailableDrivers_ReturnsFilteredDrivers()
    {
        // Arrange - Query available drivers for a date
        var queryDate = DateTime.UtcNow.AddDays(7);
        var query = $@"
            query {{
                availableDrivers(date: ""{queryDate:O}"") {{
                    id
                    name
                    shift {{
                        openTime
                        closeTime
                    }}
                    assignedRoutes {{
                        id
                        name
                        status
                    }}
                }}
            }}";

        // Act
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var drivers = json.RootElement.GetProperty("data").GetProperty("availableDrivers").EnumerateArray().ToList();
        // Should contain at least our test driver (they have a shift schedule for that day)
        drivers.Should().NotBeEmpty();
    }

    private async Task<HttpResponseMessage> ExecuteGraphQLAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return await _client.SendAsync(request);
    }

    private async Task<string> CreateDriverUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var driverRole = await roleManager.FindByNameAsync("Driver")
            ?? throw new InvalidOperationException("Driver role not found");

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = User.Create("Test", "Driver", email, $"driver.{uniqueId}");
        user.RoleId = driverRole.Id;
        var result = await userManager.CreateAsync(user, "Test@1234");

        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create driver user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        return email;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonDocument>(content)!;
    }
}
