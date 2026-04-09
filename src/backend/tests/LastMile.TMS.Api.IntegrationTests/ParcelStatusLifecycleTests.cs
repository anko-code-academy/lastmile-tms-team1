using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class ParcelStatusLifecycleTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;

    private static string AdminUsername => Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
    private static string AdminPassword => Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

    public ParcelStatusLifecycleTests(PostgreSqlContainerFixture postgreSqlFixture)
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

    private async Task<Guid> CreateTestParcelAsync()
    {
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
                trackingNumber
                status
            }
        }";

        var variables = new
        {
            input = new
            {
                description = "Lifecycle test parcel",
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "123 Shipper St",
                    city = "New York",
                    state = "NY",
                    postalCode = "10001",
                    countryCode = "US"
                },
                recipientAddress = new
                {
                    street1 = "456 Recipient St",
                    city = "Los Angeles",
                    state = "CA",
                    postalCode = "90001",
                    countryCode = "US"
                },
                weight = 1.0,
                weightUnit = "Kg",
                length = 10.0,
                width = 10.0,
                height = 10.0,
                dimensionUnit = "Cm",
                declaredValue = 100.00
            }
        };

        var json = await GraphQLRequestAsync(mutation, variables);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new InvalidOperationException($"CreateParcel failed: {errors.GetRawText()}");
        }

        var data = json.GetProperty("data").GetProperty("createParcel");
        return Guid.Parse(data.GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task ChangeParcelStatus_ValidTransition_ReturnsUpdatedStatus()
    {
        // Arrange
        var parcelId = await CreateTestParcelAsync();

        var mutation = $@"
            mutation {{
                changeParcelStatus(
                    id: ""{parcelId}"",
                    newStatus: RECEIVED_AT_DEPOT,
                    locationCity: ""Almaty"",
                    locationState: ""Almaty"",
                    locationCountry: ""KZ"",
                    description: ""Received at depot""
                ) {{
                    id
                    status
                    trackingNumber
                    deliveryAttempts
                }}
            }}";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var data = json.RootElement.GetProperty("data").GetProperty("changeParcelStatus");
        data.GetProperty("status").GetString().Should().Be("RECEIVED_AT_DEPOT");
        data.GetProperty("id").GetString().Should().Be(parcelId.ToString());
    }

    [Fact]
    public async Task ChangeParcelStatus_InvalidTransition_ReturnsError()
    {
        // Arrange - Registered cannot go directly to Delivered
        var parcelId = await CreateTestParcelAsync();

        var mutation = $@"
            mutation {{
                changeParcelStatus(
                    id: ""{parcelId}"",
                    newStatus: DELIVERED
                ) {{
                    id
                    status
                }}
            }}";

        // Act
        var response = await ExecuteGraphQLAsync(mutation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
        errors[0].GetProperty("message").GetString().Should().Contain("Cannot transition");
    }

    [Fact]
    public async Task GetTrackingEvents_ReturnsEventsForParcel()
    {
        // Arrange - Create parcel and advance through 2 statuses
        var parcelId = await CreateTestParcelAsync();

        // Advance to ReceivedAtDepot
        var transition1 = $@"
            mutation {{
                changeParcelStatus(
                    id: ""{parcelId}"",
                    newStatus: RECEIVED_AT_DEPOT,
                    description: ""Arrived at facility""
                ) {{ id }}
            }}";
        await ExecuteGraphQLAsync(transition1);

        // Advance to Sorted
        var transition2 = $@"
            mutation {{
                changeParcelStatus(
                    id: ""{parcelId}"",
                    newStatus: SORTED,
                    description: ""Sorted for dispatch""
                ) {{ id }}
            }}";
        await ExecuteGraphQLAsync(transition2);

        // Act - Query tracking events
        var query = $@"
            query {{
                trackingEvents(
                    parcelId: ""{parcelId}"",
                    first: 10
                ) {{
                    nodes {{
                        eventType
                        description
                        operator
                        timestamp
                    }}
                }}
            }}";
        var response = await ExecuteGraphQLAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var events = json.RootElement.GetProperty("data").GetProperty("trackingEvents")
            .GetProperty("nodes").EnumerateArray().ToList();
        events.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task CreateParcel_CreatesLabelCreatedEvent()
    {
        // Arrange & Act - Create a parcel
        var parcelId = await CreateTestParcelAsync();

        // Query tracking events
        var query = $@"
            query {{
                trackingEvents(
                    parcelId: ""{parcelId}"",
                    first: 10
                ) {{
                    nodes {{
                        eventType
                        description
                    }}
                }}
            }}";
        var response = await ExecuteGraphQLAsync(query);

        // Assert - Should have LABEL_CREATED event
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        var events = json.RootElement.GetProperty("data").GetProperty("trackingEvents")
            .GetProperty("nodes").EnumerateArray().ToList();
        events.Should().Contain(e => e.GetProperty("eventType").GetString() == "LABEL_CREATED");
    }

    private async Task<HttpResponseMessage> ExecuteGraphQLAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return await _client.SendAsync(request);
    }

    private async Task<JsonElement> GraphQLRequestAsync(string query, object? variables = null)
    {
        var requestBody = new { query, variables };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseContent);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonDocument>(content)!;
    }
}
