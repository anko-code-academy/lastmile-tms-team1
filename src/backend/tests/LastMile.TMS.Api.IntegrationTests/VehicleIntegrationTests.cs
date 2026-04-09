using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class VehicleIntegrationTests
{
    private readonly IntegrationFixture _fx;
    private readonly string _run;

    public VehicleIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
        _run = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ReturnsVehicle()
    {
        var mutation = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""TEST-{_run}"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.5
                ) {{
                    id
                    registrationPlate
                    type
                    parcelCapacity
                    weightCapacityKg
                    status
                }}
            }}";

        var response = await _fx.ExecuteGraphQLAsync(mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("createVehicle");
        data.GetProperty("registrationPlate").GetString().Should().Be($"TEST-{_run}");
        data.GetProperty("type").GetString().Should().Be("VAN");
        data.GetProperty("parcelCapacity").GetInt32().Should().Be(100);
        data.GetProperty("status").GetString().Should().Be("AVAILABLE");
    }

    [Fact]
    public async Task CreateVehicle_WithEmptyRegistration_ReturnsError()
    {
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

        var response = await _fx.ExecuteGraphQLAsync(mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateVehicle_StatusTransition_ValidTransitions()
    {
        var createMutation = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""UPD-{_run}"",
                    type: CAR,
                    parcelCapacity: 50,
                    weightCapacityKg: 250.0
                ) {{
                    id
                    status
                }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var vehicleId = createJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        var maintenanceMutation = $@"
            mutation {{
                changeVehicleStatus(id: ""{vehicleId}"", newStatus: MAINTENANCE) {{
                    id
                    status
                }}
            }}";

        var maintenanceResponse = await _fx.ExecuteGraphQLAsync(maintenanceMutation);

        maintenanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var maintenanceJson = await IntegrationFixture.ReadJsonAsync(maintenanceResponse);
        maintenanceJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var maintenanceData = maintenanceJson.RootElement.GetProperty("data").GetProperty("changeVehicleStatus");
        maintenanceData.GetProperty("status").GetString().Should().Be("MAINTENANCE");
    }

    [Fact]
    public async Task GetVehicles_ReturnsAllVehicles()
    {
        var createMutation1 = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""LIST1-{_run}"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.0
                ) {{ id }}
            }}";
        await _fx.ExecuteGraphQLAsync(createMutation1);

        var createMutation2 = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""LIST2-{_run}"",
                    type: CAR,
                    parcelCapacity: 50,
                    weightCapacityKg: 250.0
                ) {{ id }}
            }}";
        await _fx.ExecuteGraphQLAsync(createMutation2);

        var query = @"
            query {
                vehicles {
                    id
                    registrationPlate
                    type
                    status
                }
            }";
        var response = await _fx.ExecuteGraphQLAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var vehicles = json.RootElement.GetProperty("data").GetProperty("vehicles").EnumerateArray().ToList();
        vehicles.Should().Contain(v => v.GetProperty("registrationPlate").GetString() == $"LIST1-{_run}");
        vehicles.Should().Contain(v => v.GetProperty("registrationPlate").GetString() == $"LIST2-{_run}");
    }

    [Fact]
    public async Task GetVehicle_ById_ReturnsVehicle()
    {
        var createMutation = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""GET-{_run}"",
                    type: BIKE,
                    parcelCapacity: 10,
                    weightCapacityKg: 50.0
                ) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var vehicleId = createJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

        var query = $@"
            query {{
                vehicle(id: ""{vehicleId}"") {{
                    id
                    registrationPlate
                    type
                    status
                }}
            }}";
        var response = await _fx.ExecuteGraphQLAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var vehicle = json.RootElement.GetProperty("data").GetProperty("vehicle");
        vehicle.GetProperty("registrationPlate").GetString().Should().Be($"GET-{_run}");
        vehicle.GetProperty("type").GetString().Should().Be("BIKE");
        vehicle.GetProperty("status").GetString().Should().Be("AVAILABLE");
    }

    [Fact]
    public async Task VehicleHistory_WithJourneys_ReturnsHistory()
    {
        var createVehicleMutation = $@"
            mutation {{
                createVehicle(
                    registrationPlate: ""HIST-{_run}"",
                    type: VAN,
                    parcelCapacity: 100,
                    weightCapacityKg: 500.0
                ) {{ id }}
            }}";
        var vehicleResponse = await _fx.ExecuteGraphQLAsync(createVehicleMutation);
        var vehicleJson = await IntegrationFixture.ReadJsonAsync(vehicleResponse);
        var vehicleId = vehicleJson.RootElement.GetProperty("data").GetProperty("createVehicle").GetProperty("id").GetString();

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
        var response = await _fx.ExecuteGraphQLAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var history = json.RootElement.GetProperty("data").GetProperty("vehicleHistory");
        history.GetProperty("registrationPlate").GetString().Should().Be($"HIST-{_run}");
        history.GetProperty("totalMileageKm").GetDecimal().Should().Be(0);
        history.GetProperty("totalRoutesCompleted").GetInt32().Should().Be(0);
    }
}
