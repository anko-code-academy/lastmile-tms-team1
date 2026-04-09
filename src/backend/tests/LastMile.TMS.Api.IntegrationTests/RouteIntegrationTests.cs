using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class RouteIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationFixture _fx;
    private Guid _vehicleId = Guid.Empty;
    private string _vehiclePlate = null!;
    private Guid _driverId;

    public RouteIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
    }

    public async Task InitializeAsync()
    {
        // Create a vehicle for route assignment tests
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
        var vehicleResponse = await _fx.ExecuteGraphQLAsync(vehicleMutation);
        var vehicleJson = await IntegrationFixture.ReadJsonAsync(vehicleResponse);

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

        // Create a driver with a shift schedule for next week
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
        var driverResponse = await _fx.ExecuteGraphQLAsync(driverMutation);
        var driverJson = await IntegrationFixture.ReadJsonAsync(driverResponse);

        if (driverJson.RootElement.TryGetProperty("errors", out var driverErrors) && driverErrors.GetArrayLength() > 0)
        {
            var errorMessage = driverErrors[0].GetProperty("message").GetString();
            var fullResponse = driverJson.RootElement.GetRawText();
            throw new InvalidOperationException($"Failed to create driver in InitializeAsync: {errorMessage}. Response: {fullResponse}");
        }

        var driverData = driverJson.RootElement.GetProperty("data").GetProperty("createDriver");
        _driverId = Guid.Parse(driverData.GetProperty("id").GetString()!);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateRoute_WithValidData_ReturnsRoute()
    {
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

        var response = await _fx.ExecuteGraphQLAsync(mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = json.RootElement.GetProperty("data").GetProperty("createRoute");
        routeData.GetProperty("name").GetString().Should().Be("Test Route 001");
        routeData.GetProperty("status").GetString().Should().Be("DRAFT");
        routeData.GetProperty("totalDistanceKm").GetDecimal().Should().Be(50.5m);
        routeData.GetProperty("totalParcelCount").GetInt32().Should().Be(25);
    }

    [Fact]
    public async Task CreateRoute_WithVehicleAssignment_ReturnsRouteWithVehicle()
    {
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

        var response = await _fx.ExecuteGraphQLAsync(mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = json.RootElement.GetProperty("data").GetProperty("createRoute");
        routeData.GetProperty("vehicleId").GetString().Should().Be(_vehicleId.ToString());
        routeData.GetProperty("vehiclePlate").GetString().Should().Be(_vehiclePlate);
    }

    [Fact]
    public async Task CreateRoute_WithEmptyName_ReturnsError()
    {
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

        var response = await _fx.ExecuteGraphQLAsync(mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out var err).Should().BeTrue();
        err.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRoutes_ReturnsAllRoutes()
    {
        var createMutation1 = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route List 001"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(9):O}"",
                    totalDistanceKm: 30.0,
                    totalParcelCount: 15
                }}) {{ id }}
            }}";
        await _fx.ExecuteGraphQLAsync(createMutation1);

        var createMutation2 = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route List 002"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(9):O}"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        await _fx.ExecuteGraphQLAsync(createMutation2);

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
        var response = await _fx.ExecuteGraphQLAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routes = json.RootElement.GetProperty("data").GetProperty("routes").GetProperty("nodes").EnumerateArray().ToList();
        routes.Should().Contain(r => r.GetProperty("name").GetString() == "Route List 001");
        routes.Should().Contain(r => r.GetProperty("name").GetString() == "Route List 002");
    }

    [Fact]
    public async Task GetRoute_ById_ReturnsRoute()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Get Route By ID"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(10):O}"",
                    totalDistanceKm: 60.0,
                    totalParcelCount: 30
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

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
        var response = await _fx.ExecuteGraphQLAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var route = json.RootElement.GetProperty("data").GetProperty("route");
        route.GetProperty("name").GetString().Should().Be("Get Route By ID");
        route.GetProperty("status").GetString().Should().Be("DRAFT");
    }

    [Fact]
    public async Task UpdateRoute_ChangesRouteData()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Update Route Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(11):O}"",
                    totalDistanceKm: 25.0,
                    totalParcelCount: 10
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

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
        var updateResponse = await _fx.ExecuteGraphQLAsync(updateMutation);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(updateResponse);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = json.RootElement.GetProperty("data").GetProperty("updateRoute");
        routeData.GetProperty("name").GetString().Should().Be("Updated Route Name");
        routeData.GetProperty("totalDistanceKm").GetDecimal().Should().Be(75.0m);
        routeData.GetProperty("totalParcelCount").GetInt32().Should().Be(35);
    }

    [Fact]
    public async Task UpdateRoute_WithVehicleAssignment_UpdatesVehicleStatus()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Update Route Vehicle"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(13):O}"",
                    totalDistanceKm: 45.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

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
        var updateResponse = await _fx.ExecuteGraphQLAsync(updateMutation);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(updateResponse);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = json.RootElement.GetProperty("data").GetProperty("updateRoute");
        routeData.GetProperty("vehicleId").GetString().Should().Be(_vehicleId.ToString());
        routeData.GetProperty("vehiclePlate").GetString().Should().Be(_vehiclePlate);
    }

    [Fact]
    public async Task DeleteRoute_RemovesRoute()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Delete Route Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(14):O}"",
                    totalDistanceKm: 55.0,
                    totalParcelCount: 25
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var deleteMutation = $@"
            mutation {{
                deleteRoute(id: ""{routeId}"")
            }}";
        var deleteResponse = await _fx.ExecuteGraphQLAsync(deleteMutation);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deleteJson = await IntegrationFixture.ReadJsonAsync(deleteResponse);
        deleteJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        deleteJson.RootElement.GetProperty("data").GetProperty("deleteRoute").GetBoolean().Should().BeTrue();

        var getQuery = $@"
            query {{
                route(id: ""{routeId}"") {{
                    id
                }}
            }}";
        var getResponse = await _fx.ExecuteGraphQLAsync(getQuery);
        var getJson = await IntegrationFixture.ReadJsonAsync(getResponse);
        var routeData = getJson.RootElement.GetProperty("data").GetProperty("route");
        if (routeData.ValueKind != JsonValueKind.Null)
        {
            routeData.GetProperty("id").GetString().Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteRoute_WithAssignedVehicle_ReleasesVehicle()
    {
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
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var deleteQuery = $@"
            mutation {{
                deleteRoute(id: ""{routeId}"")
            }}";
        var deleteResponse = await _fx.ExecuteGraphQLAsync(deleteQuery);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deleteJson = await IntegrationFixture.ReadJsonAsync(deleteResponse);
        deleteJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        deleteJson.RootElement.GetProperty("data").GetProperty("deleteRoute").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ChangeRouteStatus_ToInProgress_SetsActualStartTime()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Status Change Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(12):O}"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var changeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{
                    id
                    status
                    actualStartTime
                }}
            }}";
        var changeResponse = await _fx.ExecuteGraphQLAsync(changeMutation);

        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var changeJson = await IntegrationFixture.ReadJsonAsync(changeResponse);
        changeJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = changeJson.RootElement.GetProperty("data").GetProperty("changeRouteStatus");
        routeData.GetProperty("status").GetString().Should().Be("IN_PROGRESS");
        routeData.GetProperty("actualStartTime").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChangeRouteStatus_ToCompleted_ReleasesVehicle()
    {
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
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var startMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{
                    id
                    status
                }}
            }}";
        await _fx.ExecuteGraphQLAsync(startMutation);

        var completeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: COMPLETED) {{
                    id
                    status
                    actualEndTime
                    vehiclePlate
                }}
            }}";
        var completeResponse = await _fx.ExecuteGraphQLAsync(completeMutation);

        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeJson = await IntegrationFixture.ReadJsonAsync(completeResponse);
        completeJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = completeJson.RootElement.GetProperty("data").GetProperty("changeRouteStatus");
        routeData.GetProperty("status").GetString().Should().Be("COMPLETED");
        routeData.GetProperty("actualEndTime").GetString().Should().NotBeNullOrEmpty();
        routeData.GetProperty("vehiclePlate").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChangeRouteStatus_ToCompleted_UpdatesVehicleHistory()
    {
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
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var startMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{
                    id
                    status
                }}
            }}";
        await _fx.ExecuteGraphQLAsync(startMutation);

        var completeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: COMPLETED) {{
                    id
                    status
                }}
            }}";
        await _fx.ExecuteGraphQLAsync(completeMutation);

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
        var historyResponse = await _fx.ExecuteGraphQLAsync(historyQuery);
        var historyJson = await IntegrationFixture.ReadJsonAsync(historyResponse);
        historyJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();

        var historyData = historyJson.RootElement.GetProperty("data").GetProperty("vehicleHistory");
        historyData.GetProperty("totalRoutesCompleted").GetInt32().Should().BeGreaterThan(0);
        var routes = historyData.GetProperty("routes").EnumerateArray().ToList();
        routes.Should().Contain(r => r.GetProperty("routeId").GetString() == routeId);
    }

    [Fact]
    public async Task CreateRoute_WithRetiredVehicle_ReturnsError()
    {
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
        var vehicleResponse = await _fx.ExecuteGraphQLAsync(createVehicleMutation);
        var vehicleJson = await IntegrationFixture.ReadJsonAsync(vehicleResponse);
        var vehicleId = vehicleJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        var retireMutation = $@"
            mutation {{
                changeVehicleStatus(id: ""{vehicleId}"", newStatus: RETIRED) {{
                    id
                    status
                }}
            }}";
        await _fx.ExecuteGraphQLAsync(retireMutation);

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
        var routeResponse = await _fx.ExecuteGraphQLAsync(createRouteMutation);

        routeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var routeJson = await IntegrationFixture.ReadJsonAsync(routeResponse);
        routeJson.RootElement.TryGetProperty("errors", out var err).Should().BeTrue();
        err.GetArrayLength().Should().BeGreaterThan(0);
        err[0].GetProperty("message").GetString().Should().Contain("retired vehicle");
    }

    [Fact]
    public async Task CreateRoute_WithDriverAssignment_ReturnsDriverFields()
    {
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

        var response = await _fx.ExecuteGraphQLAsync(mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = json.RootElement.GetProperty("data").GetProperty("createRoute");
        routeData.GetProperty("driverId").GetString().Should().Be(_driverId.ToString());
        routeData.GetProperty("driverName").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignDriverToRoute_OnPlannedRoute_Succeeds()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Assign Driver Test"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(8):O}"",
                    totalDistanceKm: 40.0,
                    totalParcelCount: 20
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

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
        var assignResponse = await _fx.ExecuteGraphQLAsync(assignMutation);

        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignJson = await IntegrationFixture.ReadJsonAsync(assignResponse);
        assignJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = assignJson.RootElement.GetProperty("data").GetProperty("assignDriverToRoute");
        routeData.GetProperty("driverId").GetString().Should().Be(_driverId.ToString());
        routeData.GetProperty("driverName").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignDriverToRoute_Reassignment_Succeeds()
    {
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
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

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
        var driver2Response = await _fx.ExecuteGraphQLAsync(createDriver2Mutation);
        var driver2Json = await IntegrationFixture.ReadJsonAsync(driver2Response);
        var driver2Id = driver2Json.RootElement.GetProperty("data").GetProperty("createDriver").GetProperty("id").GetString();

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
        var reassignResponse = await _fx.ExecuteGraphQLAsync(reassignMutation);

        reassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reassignJson = await IntegrationFixture.ReadJsonAsync(reassignResponse);
        reassignJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = reassignJson.RootElement.GetProperty("data").GetProperty("assignDriverToRoute");
        routeData.GetProperty("driverId").GetString().Should().Be(driver2Id);
    }

    [Fact]
    public async Task AssignDriverToRoute_Unassign_SetsDriverToNull()
    {
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
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var unassignMutation = $@"
            mutation {{
                assignDriverToRoute(
                    routeId: ""{routeId}""
                ) {{
                    id
                    driverId
                }}
            }}";
        var unassignResponse = await _fx.ExecuteGraphQLAsync(unassignMutation);

        unassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var unassignJson = await IntegrationFixture.ReadJsonAsync(unassignResponse);
        unassignJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeData = unassignJson.RootElement.GetProperty("data").GetProperty("assignDriverToRoute");
        routeData.TryGetProperty("driverId", out var driverIdProp).Should().BeTrue();
        driverIdProp.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task AssignDriverToRoute_OnCompletedRoute_ReturnsError()
    {
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Completed Route Driver"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(11):O}"",
                    totalDistanceKm: 35.0,
                    totalParcelCount: 15
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        var startMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: IN_PROGRESS) {{ id }}
            }}";
        await _fx.ExecuteGraphQLAsync(startMutation);

        var completeMutation = $@"
            mutation {{
                changeRouteStatus(id: ""{routeId}"", newStatus: COMPLETED) {{ id }}
            }}";
        await _fx.ExecuteGraphQLAsync(completeMutation);

        var assignMutation = $@"
            mutation {{
                assignDriverToRoute(
                    routeId: ""{routeId}"",
                    driverId: ""{_driverId}""
                ) {{
                    id
                }}
            }}";
        var assignResponse = await _fx.ExecuteGraphQLAsync(assignMutation);

        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignJson = await IntegrationFixture.ReadJsonAsync(assignResponse);
        assignJson.RootElement.TryGetProperty("errors", out var err).Should().BeTrue();
        err[0].GetProperty("message").GetString().Should().Contain("Draft");
    }

    [Fact]
    public async Task GetAvailableDrivers_ReturnsFilteredDrivers()
    {
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

        var response = await _fx.ExecuteGraphQLAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var drivers = json.RootElement.GetProperty("data").GetProperty("availableDrivers").EnumerateArray().ToList();
        drivers.Should().NotBeEmpty();
    }

    private async Task<string> CreateDriverUserAsync(string email)
    {
        using var scope = _fx.CreateScope();
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

    [Fact]
    public async Task OptimizeRouteStopOrder_WithStops_ReturnsOptimizedRoute()
    {
        // Arrange - Create a route, then seed zone/depot/stops directly via DB
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Optimize Test Route"",
                    plannedStartTime: ""{DateTime.UtcNow.AddDays(10):O}""
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        createJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var routeId = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Seed depot, zone, and stops via DB context
        using (var scope = _fx.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LastMile.TMS.Persistence.AppDbContext>();

            var depot = new Depot
            {
                Name = "Optimize Test Depot",
                Address = new Address
                {
                    Street1 = "1 Depot St",
                    City = "Test City",
                    GeoLocation = new Point(0, 0) { SRID = 4326 }
                }
            };
            db.Depots.Add(depot);
            await db.SaveChangesAsync();

            var zone = new Zone
            {
                Name = "Optimize Test Zone",
                DepotId = depot.Id
            };
            db.Zones.Add(zone);
            await db.SaveChangesAsync();

            // Assign zone to route
            var route = db.Routes.First(r => r.Id == Guid.Parse(routeId!));
            route.ZoneId = zone.Id;
            await db.SaveChangesAsync();

            // Add stops with varying distances from depot
            var stops = new[]
            {
                new { Lat = 0.04, Lon = 0.04, Street = "Far Stop" },
                new { Lat = 0.01, Lon = 0.01, Street = "Near Stop" },
                new { Lat = 0.02, Lon = 0.02, Street = "Mid Stop" },
            };

            for (int i = 0; i < stops.Length; i++)
            {
                db.RouteStops.Add(new RouteStop
                {
                    SequenceNumber = i + 1,
                    Street1 = stops[i].Street,
                    GeoLocation = new Point(stops[i].Lon, stops[i].Lat) { SRID = 4326 },
                    RouteId = Guid.Parse(routeId!)
                });
            }
            await db.SaveChangesAsync();
        }

        // Act
        var optimizeMutation = $@"
            mutation {{
                optimizeRouteStopOrder(input: {{ routeId: ""{routeId}"" }}) {{
                    id
                    name
                    status
                    stops {{
                        id
                        sequenceNumber
                        street1
                    }}
                }}
            }}";
        var optimizeResponse = await _fx.ExecuteGraphQLAsync(optimizeMutation);

        // Assert
        optimizeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var optimizeJson = await IntegrationFixture.ReadJsonAsync(optimizeResponse);
        optimizeJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();

        var data = optimizeJson.RootElement.GetProperty("data").GetProperty("optimizeRouteStopOrder");
        data.GetProperty("id").GetString().Should().Be(routeId);

        var returnedStops = data.GetProperty("stops").EnumerateArray().ToList();
        returnedStops.Should().HaveCount(3);

        // Verify sequence numbers are 1, 2, 3
        var sequences = returnedStops.Select(s => s.GetProperty("sequenceNumber").GetInt32()).ToList();
        sequences.Should().Equal([1, 2, 3]);

        // The nearest stop to depot should be first
        returnedStops[0].GetProperty("street1").GetString().Should().Be("Near Stop");
    }
}
