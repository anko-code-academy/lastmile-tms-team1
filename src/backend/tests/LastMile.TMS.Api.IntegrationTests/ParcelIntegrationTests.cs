using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class ParcelIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;

    public ParcelIntegrationTests(PostgreSqlContainerFixture postgreSqlFixture)
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
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<JsonElement> GraphQLRequestAsync(string query, object? variables = null)
    {
        var requestBody = new { query, variables };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql") { Content = content };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseContent);
    }

    [Fact]
    public async Task CreateParcel_WithValidInput_ReturnsParcelWithTrackingNumber()
    {
        // Arrange
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
                trackingNumber
                status
                serviceType
                createdAt
            }
        }";

        var variables = new
        {
            input = new
            {
                description = "Test parcel",
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "123 Sender St",
                    city = "New York",
                    state = "NY",
                    postalCode = "10001",
                    countryCode = "US",
                    contactName = "John Sender",
                    phone = "+1-555-0100",
                    email = "sender@example.com"
                },
                recipientAddress = new
                {
                    street1 = "456 Recipient Ave",
                    city = "Los Angeles",
                    state = "CA",
                    postalCode = "90001",
                    countryCode = "US",
                    contactName = "Jane Recipient",
                    phone = "+1-555-0200",
                    email = "recipient@example.com"
                },
                weight = 2.5,
                weightUnit = "Lb",
                length = 10.0,
                width = 8.0,
                height = 5.0,
                dimensionUnit = "In",
                declaredValue = 100.00,
                currency = "USD",
                parcelType = "General",
                notes = "Handle with care"
            }
        };

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation, variables);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcel = jsonResponse.GetProperty("data").GetProperty("createParcel");
        parcel.TryGetProperty("id", out _).Should().BeTrue();
        parcel.GetProperty("trackingNumber").GetString().Should().NotBeNullOrEmpty();
        parcel.GetProperty("trackingNumber").GetString().Should().StartWith("LM-");
        parcel.GetProperty("status").GetString().Should().Be("REGISTERED");
        parcel.GetProperty("serviceType").GetString().Should().Be("STANDARD");
    }

    [Fact]
    public async Task CreateParcel_WithMissingRequiredFields_ReturnsValidationError()
    {
        // Arrange - missing weight, dimensions, and addresses
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
                trackingNumber
            }
        }";

        var variables = new
        {
            input = new
            {
                description = "Invalid parcel",
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "",
                    city = "",
                    state = "",
                    postalCode = "",
                    countryCode = "US"
                },
                recipientAddress = new
                {
                    street1 = "",
                    city = "",
                    state = "",
                    postalCode = "",
                    countryCode = "US"
                },
                weight = 0,
                weightUnit = "Lb",
                length = 0,
                width = 0,
                height = 0,
                dimensionUnit = "In"
            }
        };

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation, variables);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateParcel_WithInvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
            }
        }";

        var variables = new
        {
            input = new
            {
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "123 Sender St",
                    city = "New York",
                    state = "NY",
                    postalCode = "10001",
                    countryCode = "US",
                    email = "not-an-email"
                },
                recipientAddress = new
                {
                    street1 = "456 Recipient Ave",
                    city = "Los Angeles",
                    state = "CA",
                    postalCode = "90001",
                    countryCode = "US"
                },
                weight = 2.5,
                weightUnit = "Lb",
                length = 10.0,
                width = 8.0,
                height = 5.0,
                dimensionUnit = "In"
            }
        };

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation, variables);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateParcel_GeneratesUniqueTrackingNumbers()
    {
        // Arrange
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                trackingNumber
            }
        }";

        var baseInput = new
        {
            serviceType = "Standard",
            shipperAddress = new
            {
                street1 = "123 Sender St",
                city = "New York",
                state = "NY",
                postalCode = "10001",
                countryCode = "US"
            },
            recipientAddress = new
            {
                street1 = "456 Recipient Ave",
                city = "Los Angeles",
                state = "CA",
                postalCode = "90001",
                countryCode = "US"
            },
            weight = 1.0,
            weightUnit = "Lb",
            length = 5.0,
            width = 5.0,
            height = 5.0,
            dimensionUnit = "In"
        };

        // Act - create two parcels
        var response1 = await GraphQLRequestAsync(mutation, new { input = baseInput });
        var response2 = await GraphQLRequestAsync(mutation, new { input = baseInput });

        // Assert
        var tracking1 = response1.GetProperty("data").GetProperty("createParcel").GetProperty("trackingNumber").GetString();
        var tracking2 = response2.GetProperty("data").GetProperty("createParcel").GetProperty("trackingNumber").GetString();

        tracking1.Should().NotBe(tracking2, "each parcel should get a unique tracking number");
    }

    private static async Task CleanupTestDataAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Depots\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Parcel\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Addresses\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
