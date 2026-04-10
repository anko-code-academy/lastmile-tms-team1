using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class BinIntegrationTests
{
    private readonly IntegrationFixture _fx;

    public BinIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
    }

    private Task<JsonElement> GqlAsync(string query) => _fx.GraphQLRequestAsync(query);

    private static int _counter;
    private async Task<(string depotId, string zoneId, string aisleId)> CreateDepotZoneAisleAsync()
    {
        // Use a counter-based prefix to guarantee unique first-char for bin labels
        int c = Interlocked.Increment(ref _counter);
        char letter = (char)('C' + (c % 20)); // C..V — 20 unique letters
        var run = $"{letter}{Guid.NewGuid():N}"[..10];
        var depotMutation = $@"mutation {{
            createDepot(input: {{
                name: ""{run}"",
                address: {{ street1: ""123 Test St"", city: ""Test City"", state: ""TS"", postalCode: ""12345"", countryCode: ""US"" }}
            }}) {{ id }}
        }}";
        var depotJson = await GqlAsync(depotMutation);
        var depotId = depotJson.GetProperty("data").GetProperty("createDepot").GetProperty("id").GetString()!;

        var geoJson = @"{""type"":""Polygon"",""coordinates"":[[[-122.4194,37.7749],[-122.4094,37.7749],[-122.4094,37.7849],[-122.4194,37.7849],[-122.4194,37.7749]]]}";
        var zoneMutation = $@"mutation {{
            createZone(input: {{
                name: ""B"",
                geoJson: ""{geoJson.Replace("\"", "\\\"")}"",
                depotId: ""{depotId}""
            }}) {{ id }}
        }}";
        var zoneJson = await GqlAsync(zoneMutation);
        var zoneId = zoneJson.GetProperty("data").GetProperty("createZone").GetProperty("id").GetString()!;

        var aisleMutation = $@"mutation {{
            createAisle(input: {{ name: ""A1"", zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var aisleJson = await GqlAsync(aisleMutation);
        var aisleId = aisleJson.GetProperty("data").GetProperty("createAisle").GetProperty("id").GetString()!;

        return (depotId, zoneId, aisleId);
    }

    // --- Bin Tests ---

    [Fact]
    public async Task CreateBin_WithValidInput_ReturnsBin()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var mutation = $@"mutation {{
            createBin(input: {{
                aisleId: ""{aisleId}"",
                slot: 1,
                capacity: 50,
                zoneId: ""{zoneId}"",
                isActive: true
            }}) {{
                id
                label
                aisleLabel
                slot
                capacity
                isActive
                zoneId
            }}
        }}";

        var json = await GqlAsync(mutation);

        var bin = json.GetProperty("data").GetProperty("createBin");
        bin.TryGetProperty("id", out _).Should().BeTrue();
        bin.GetProperty("slot").GetInt32().Should().Be(1);
        bin.GetProperty("capacity").GetInt32().Should().Be(50);
        bin.GetProperty("isActive").GetBoolean().Should().BeTrue();
        bin.GetProperty("zoneId").GetString().Should().Be(zoneId);
    }

    [Fact]
    public async Task CreateBin_LabelFormat_ContainsAisleAndSlot()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var mutation = $@"mutation {{
            createBin(input: {{
                aisleId: ""{aisleId}"",
                slot: 2,
                capacity: 25,
                zoneId: ""{zoneId}""
            }}) {{
                id label
            }}
        }}";

        var json = await GqlAsync(mutation);
        var label = json.GetProperty("data").GetProperty("createBin").GetProperty("label").GetString();
        label.Should().StartWith("D");
        label.Should().EndWith("-02");
    }

    [Fact]
    public async Task CreateBin_WithInvalidZone_ReturnsError()
    {
        var (_, _, aisleId) = await CreateDepotZoneAisleAsync();
        var fakeZoneId = Guid.NewGuid();

        var mutation = $@"mutation {{
            createBin(input: {{
                aisleId: ""{aisleId}"",
                slot: 1,
                capacity: 50,
                zoneId: ""{fakeZoneId}""
            }}) {{ id }}
        }}";

        var json = await GqlAsync(mutation);
        json.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateBin_DuplicateLabel_ReturnsError()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 10, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var first = await GqlAsync(createMutation);
        first.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString().Should().NotBeNull();

        var duplicateMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 10, capacity: 100, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var second = await GqlAsync(duplicateMutation);
        second.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateBin_ValidInput_ReturnsUpdatedBin()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var createJson = await GqlAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateBin(input: {{
                id: ""{binId}"",
                aisleId: ""{aisleId}"",
                slot: 5,
                capacity: 100,
                zoneId: ""{zoneId}"",
                isActive: false
            }}) {{
                id slot capacity isActive
            }}
        }}";

        var updateJson = await GqlAsync(updateMutation);
        var updated = updateJson.GetProperty("data").GetProperty("updateBin");
        updated.GetProperty("slot").GetInt32().Should().Be(5);
        updated.GetProperty("capacity").GetInt32().Should().Be(100);
        updated.GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBin_ExistingBin_ReturnsTrue()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var createJson = await GqlAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        var deleteMutation = $@"mutation {{ deleteBin(id: ""{binId}"") }}";
        var deleteJson = await GqlAsync(deleteMutation);
        deleteJson.GetProperty("data").GetProperty("deleteBin").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBin_NonExistingBin_ReturnsFalse()
    {
        var deleteMutation = $@"mutation {{ deleteBin(id: ""{Guid.NewGuid()}"") }}";
        var json = await GqlAsync(deleteMutation);
        json.GetProperty("data").GetProperty("deleteBin").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task QueryBin_ById_ReturnsBin()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 5, capacity: 75, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var createJson = await GqlAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        var query = $@"query {{
            bin(id: ""{binId}"") {{ id label slot capacity isActive aisle {{ label }} }}
        }}";
        var queryJson = await GqlAsync(query);

        var bin = queryJson.GetProperty("data").GetProperty("bin");
        bin.GetProperty("slot").GetInt32().Should().Be(5);
        bin.GetProperty("capacity").GetInt32().Should().Be(75);
    }

    [Fact]
    public async Task QueryBins_ReturnsAllBins()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        await GqlAsync($@"mutation {{ createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }} }}");
        await GqlAsync($@"mutation {{ createBin(input: {{ aisleId: ""{aisleId}"", slot: 2, capacity: 60, zoneId: ""{zoneId}"" }}) {{ id }} }}");

        var query = @"query { bins(first: 50) { nodes { id label } } }";
        var queryJson = await GqlAsync(query);

        var bins = queryJson.GetProperty("data").GetProperty("bins").GetProperty("nodes");
        bins.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task QueryBinUtilizations_ReturnsUtilizationData()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        await GqlAsync($@"mutation {{ createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }} }}");

        var query = $@"query {{
            binUtilizations(zoneId: ""{zoneId}"") {{
                id label slot capacity utilizationPercent isActive
            }}
        }}";
        var queryJson = await GqlAsync(query);

        var utilizations = queryJson.GetProperty("data").GetProperty("binUtilizations");
        utilizations.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    // --- Aisle Tests ---

    [Fact]
    public async Task CreateAisle_WithValidInput_ReturnsAisle()
    {
        var (_, zoneId, _) = await CreateDepotZoneAisleAsync();

        // Create an additional aisle to test
        var aisleMutation = $@"mutation {{
            createAisle(input: {{ name: ""A2"", zoneId: ""{zoneId}"" }}) {{ id name label isActive }}
        }}";
        var aisleJson = await GqlAsync(aisleMutation);

        var aisle = aisleJson.GetProperty("data").GetProperty("createAisle");
        aisle.TryGetProperty("id", out _).Should().BeTrue();
        aisle.GetProperty("name").GetString().Should().Be("A2");
        aisle.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAisle_WithValidInput_ReturnsUpdatedAisle()
    {
        var (_, zoneId, _) = await CreateDepotZoneAisleAsync();

        // Create a separate aisle to update
        var createMutation = $@"mutation {{ createAisle(input: {{ name: ""A2"", zoneId: ""{zoneId}"" }}) {{ id }} }}";
        var createJson = await GqlAsync(createMutation);
        var aisleId = createJson.GetProperty("data").GetProperty("createAisle").GetProperty("id").GetString();

        var updateMutation = $@"mutation {{
            updateAisle(input: {{
                id: ""{aisleId}"",
                name: ""A3"",
                zoneId: ""{zoneId}"",
                order: 1,
                isActive: false
            }}) {{ id name isActive }}
        }}";
        var updateJson = await GqlAsync(updateMutation);

        var aisle = updateJson.GetProperty("data").GetProperty("updateAisle");
        aisle.GetProperty("id").GetString().Should().Be(aisleId);
        aisle.GetProperty("name").GetString().Should().Be("A3");
        aisle.GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAisle_WithValidId_ReturnsTrue()
    {
        var (_, zoneId, _) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{ createAisle(input: {{ name: ""A2"", zoneId: ""{zoneId}"" }}) {{ id }} }}";
        var createJson = await GqlAsync(createMutation);
        var aisleId = createJson.GetProperty("data").GetProperty("createAisle").GetProperty("id").GetString();

        var deleteMutation = $@"mutation {{ deleteAisle(id: ""{aisleId}"") }}";
        var deleteJson = await GqlAsync(deleteMutation);
        deleteJson.GetProperty("data").GetProperty("deleteAisle").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetAisles_FilteredByZone_ReturnsAisles()
    {
        var (_, zoneId, _) = await CreateDepotZoneAisleAsync();

        // Create a second aisle (first already created in helper)
        await GqlAsync($@"mutation {{ createAisle(input: {{ name: ""A2"", zoneId: ""{zoneId}"" }}) {{ id }} }}");

        var query = $@"query {{ aisles(where: {{ zoneId: {{ eq: ""{zoneId}"" }} }}, order: {{ name: ASC }}) {{ edges {{ node {{ id name label }} }} }} }}";
        var queryJson = await GqlAsync(query);

        var edges = queryJson.GetProperty("data").GetProperty("aisles").GetProperty("edges");
        edges.GetArrayLength().Should().Be(2);
        edges[0].GetProperty("node").GetProperty("name").GetString().Should().Be("A1");
        edges[1].GetProperty("node").GetProperty("name").GetString().Should().Be("A2");
    }

    // --- Label Tests ---

    [Fact]
    public async Task GetBinPdfLabel_ReturnsPdfFile()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var createJson = await GqlAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        var pdfResponse = await _fx.Client.GetAsync($"/labels/bin/{binId}/pdf");
        pdfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pdfResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Take(4).Should().Contain([0x25, 0x50, 0x46, 0x25]); // PDF magic number
    }

    [Fact]
    public async Task GetBinPngLabel_ReturnsPngFile()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var createJson = await GqlAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        var pngResponse = await _fx.Client.GetAsync($"/labels/bin/{binId}/png");
        pngResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pngResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/png");

        var pngBytes = await pngResponse.Content.ReadAsByteArrayAsync();
        pngBytes.Should().NotBeEmpty();
        pngBytes.Take(8).Should().Contain([0x89, 0x50, 0x4E, 0x47]); // PNG magic number
    }

    [Fact]
    public async Task GetBinZplLabel_ReturnsZplText()
    {
        var (_, zoneId, aisleId) = await CreateDepotZoneAisleAsync();

        var createMutation = $@"mutation {{
            createBin(input: {{ aisleId: ""{aisleId}"", slot: 1, capacity: 50, zoneId: ""{zoneId}"" }}) {{ id }}
        }}";
        var createJson = await GqlAsync(createMutation);
        var binId = createJson.GetProperty("data").GetProperty("createBin").GetProperty("id").GetString();

        var response = await _fx.Client.GetAsync($"/labels/bin/{binId}/zpl");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBinPdfLabel_InvalidBinId_ReturnsNotFound()
    {
        var response = await _fx.Client.GetAsync($"/labels/bin/{Guid.NewGuid()}/pdf");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBinPngLabel_InvalidBinId_ReturnsNotFound()
    {
        var response = await _fx.Client.GetAsync($"/labels/bin/{Guid.NewGuid()}/png");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBinZplLabel_InvalidBinId_ReturnsNotFound()
    {
        var response = await _fx.Client.GetAsync($"/labels/bin/{Guid.NewGuid()}/zpl");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
