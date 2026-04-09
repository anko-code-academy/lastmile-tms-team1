using System.Text.Json;
using FluentAssertions;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class DepotZoneIntegrationTests
{
    private readonly IntegrationFixture _fx;
    private readonly string _run;

    public DepotZoneIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
        _run = Guid.NewGuid().ToString("N")[..8];
    }

    private Task<JsonElement> GqlAsync(string query) => _fx.GraphQLRequestAsync(query);

    [Fact]
    public async Task CreateDepot_WithValidInput_ReturnsDepot()
    {
        var mutation = $@"mutation {{
            createDepot(input: {{
                name: ""Test Depot {_run}"",
                address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
                isActive
                createdAt
            }}
        }}";

        var jsonResponse = await GqlAsync(mutation);

        jsonResponse.GetProperty("data").GetProperty("createDepot").TryGetProperty("id", out _).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createDepot").GetProperty("name").GetString().Should().Be($"Test Depot {_run}");
        jsonResponse.GetProperty("data").GetProperty("createDepot").GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateDepot_WithMissingName_ReturnsValidationError()
    {
        var mutation = @"mutation {
            createDepot(input: {
                name: """",
                address: { street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }
            }) {
                id
                name
            }
        }";

        var jsonResponse = await GqlAsync(mutation);

        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateDepot_WithValidInput_ReturnsUpdatedDepot()
    {
        var createMutation = $@"mutation {{
            createDepot(input: {{
                name: ""Original Depot {_run}"",
                address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }}
            }}) {{
                id
                name
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Updated Depot {_run}"",
                address: {{ street1: ""456 Update St"", city: ""Update City"", state: ""US"", postalCode: ""54321"", countryCode: ""US"" }},
                isActive: false
            }}) {{
                id
                name
                isActive
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);

        updateJson.GetProperty("data").GetProperty("updateDepot").GetProperty("name").GetString().Should().Be($"Updated Depot {_run}");
        updateJson.GetProperty("data").GetProperty("updateDepot").GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task CreateZone_WithValidGeoJson_ReturnsZone()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{ name: ""Zone Test Depot {_run}"", address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }} }}) {{
                id
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var mutation = $@"mutation {{
            createZone(input: {{
                name: ""Test Zone {_run}"",
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

        var jsonResponse = await GqlAsync(mutation);

        jsonResponse.GetProperty("data").GetProperty("createZone").TryGetProperty("id", out _).Should().BeTrue();
        jsonResponse.GetProperty("data").GetProperty("createZone").GetProperty("name").GetString().Should().Be($"Test Zone {_run}");
        jsonResponse.GetProperty("data").GetProperty("createZone").GetProperty("depotId").GetString().Should().Be(depotId);
    }

    [Fact]
    public async Task CreateZone_WithInvalidGeoJson_ReturnsValidationError()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{ name: ""Invalid GeoJSON Depot {_run}"", address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }} }}) {{
                id
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var invalidGeoJson = @"{""type"":""Point"",""coordinates"":[-122.4194,37.7749]}";
        var mutation = $@"mutation {{
            createZone(input: {{
                name: ""Invalid Zone {_run}"",
                geoJson: ""{invalidGeoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
            }}
        }}";

        var jsonResponse = await GqlAsync(mutation);

        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateZone_LinkedToDepot_ReturnsZoneWithDepot()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{ name: ""Link Test Depot {_run}"", address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }} }}) {{
                id
                name
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Linked Zone {_run}"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
                depotId
            }}
        }}";

        var zoneJson = await GqlAsync(createZoneMutation);

        zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("depotId").GetString().Should().Be(depotId);
    }

    [Fact]
    public async Task UpdateZone_WithValidInput_ReturnsUpdatedZone()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{ name: ""Update Zone Depot {_run}"", address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }} }}) {{
                id
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Original Zone {_run}"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
            }}
        }}";

        var zoneJson = await GqlAsync(createZoneMutation);
        var zoneId = zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateZone(input: {{
                id: ""{zoneId}"",
                name: ""Updated Zone {_run}"",
                depotId: ""{depotId}"",
                isActive: false
            }}) {{
                id
                name
                isActive
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);

        updateJson.GetProperty("data").GetProperty("updateZone").GetProperty("name").GetString().Should().Be($"Updated Zone {_run}");
        updateJson.GetProperty("data").GetProperty("updateZone").GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task QueryDepot_ById_ReturnsDepot()
    {
        var createMutation = $@"mutation {{
            createDepot(input: {{
                name: ""Query Test Depot {_run}"",
                address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var query = $@"query {{
            depot(id: ""{depotId}"") {{
                id
                name
                isActive
                createdAt
            }}
        }}";

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out _))
        {
            throw new Exception($"GraphQL errors: {queryJson.GetProperty("errors").GetRawText()}");
        }

        queryJson.GetProperty("data").GetProperty("depot").GetProperty("name").GetString().Should().Be($"Query Test Depot {_run}");
        queryJson.GetProperty("data").GetProperty("depot").GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task QueryZones_ReturnsAllZones()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{ name: ""All Zones Depot {_run}"", address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }} }}) {{
                id
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Test Zone 1 {_run}"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
            }}
        }}";

        await GqlAsync(createZoneMutation);

        var query = @"query {
            zones(first: 50) {
                nodes {
                    id
                    name
                    depotId
                    isActive
                }
            }
        }";

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out _))
        {
            throw new Exception($"GraphQL errors: {queryJson.GetProperty("errors").GetRawText()}");
        }

        var zones = queryJson.GetProperty("data").GetProperty("zones").GetProperty("nodes");
        zones.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateDepot_CanBeQueriedById_AfterCreation()
    {
        var createMutation = $@"mutation {{
            createDepot(input: {{
                name: ""Persist Test Depot {_run}"",
                address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        var createJson = await GqlAsync(createMutation);

        if (createJson.TryGetProperty("errors", out var createErrors))
        {
            throw new Exception($"Create depot failed: {createErrors.GetRawText()}");
        }

        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();
        depotId.Should().NotBeNullOrEmpty("depot should be created with an ID");

        var query = @"query {
            depots(first: 50) {
                nodes {
                    id
                    name
                }
            }
        }";

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query depots failed: {queryErrors.GetRawText()}");
        }

        var depots = queryJson.GetProperty("data").GetProperty("depots").GetProperty("nodes");
        var depotIds = depots.EnumerateArray().Select(d => d.GetProperty("id").GetString()).ToList();

        depotIds.Should().Contain(depotId, $"Created depot with ID {depotId} should be queryable after creation. Found IDs: {string.Join(", ", depotIds)}");
    }

    [Fact]
    public async Task UpdateDepot_Address_ShouldPersistNewAddress()
    {
        var createMutation = $@"mutation {{
            createDepot(input: {{
                name: ""Address Update Depot {_run}"",
                address: {{ street1: ""100 Old St"", city: ""Old City"", state: ""OC"", postalCode: ""11111"", countryCode: ""US"" }}
            }}) {{
                id
                name
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Address Update Depot {_run}"",
                address: {{ street1: ""200 New St"", city: ""New City"", state: ""NC"", postalCode: ""22222"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);

        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update depot failed: {errors.GetRawText()}");
        }

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

        var queryJson = await GqlAsync(query);

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
        var createMutation = $@"mutation {{
            createDepot(input: {{
                name: ""Schedule Update Depot {_run}"",
                address: {{ street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }}
            }}) {{
                id
                name
            }}
        }}";

        var createJson = await GqlAsync(createMutation);
        var depotId = createJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Schedule Update Depot {_run}"",
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

        var updateJson = await GqlAsync(updateMutation);

        if (updateJson.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update depot schedule failed: {errors.GetRawText()}");
        }

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

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query depot failed: {queryErrors.GetRawText()}");
        }

        var depot = queryJson.GetProperty("data").GetProperty("depot");
        var schedules = depot.GetProperty("shiftSchedules");

        var saturdaySchedule = schedules.EnumerateArray().FirstOrDefault(s => s.GetProperty("dayOfWeek").GetString() == "SATURDAY");
        saturdaySchedule.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        saturdaySchedule.GetProperty("openTime").GetString().Should().Be("08:00:00");
        saturdaySchedule.GetProperty("closeTime").GetString().Should().Be("20:00:00");

        var fridaySchedule = schedules.EnumerateArray().FirstOrDefault(s => s.GetProperty("dayOfWeek").GetString() == "FRIDAY");
        fridaySchedule.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        fridaySchedule.GetProperty("openTime").GetString().Should().Be("08:00:00");
        fridaySchedule.GetProperty("closeTime").GetString().Should().Be("20:00:00");

        var sundaySchedule = schedules.EnumerateArray().FirstOrDefault(s => s.GetProperty("dayOfWeek").GetString() == "SUNDAY");
        sundaySchedule.ValueKind.Should().Be(JsonValueKind.Undefined);
    }

    [Fact]
    public async Task UpdateDepot_Name_ZoneDepotName_ShouldReflectChange()
    {
        var createDepotMutation = $@"mutation {{
            createDepot(input: {{
                name: ""Original Depot Name {_run}"",
                address: {{ street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }}
            }}) {{
                id
                name
            }}
        }}";

        var depotJson = await GqlAsync(createDepotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString();

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var createZoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""Test Zone {_run}"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{
                id
                name
            }}
        }}";

        await GqlAsync(createZoneMutation);

        var updateMutation = $@"mutation {{
            updateDepot(input: {{
                id: ""{depotId}"",
                name: ""Updated Depot Name {_run}"",
                address: {{ street1: ""100 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }},
                isActive: true
            }}) {{
                id
                name
            }}
        }}";

        await GqlAsync(updateMutation);

        var query = $@"query {{
            zones(first: 50) {{
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

        var queryJson = await GqlAsync(query);

        if (queryJson.TryGetProperty("errors", out var queryErrors))
        {
            throw new Exception($"Query zones failed: {queryErrors.GetRawText()}");
        }

        var zones = queryJson.GetProperty("data").GetProperty("zones").GetProperty("nodes");
        var testZone = zones.EnumerateArray().FirstOrDefault(z => z.GetProperty("name").GetString() == $"Test Zone {_run}");

        testZone.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        testZone.GetProperty("depot").GetProperty("name").GetString().Should().Be($"Updated Depot Name {_run}");
    }
}
