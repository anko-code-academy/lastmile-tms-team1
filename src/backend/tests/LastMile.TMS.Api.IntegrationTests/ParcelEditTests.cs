using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class ParcelEditTests : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _accessToken = null!;

    public ParcelEditTests(PostgreSqlContainerFixture postgreSqlFixture)
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

        // Login to get access token
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

        // Seed test data
        await SeedTestDataAsync();
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
        return JsonSerializer.Deserialize<JsonElement>(responseContent)!;
    }

    private async Task SeedTestDataAsync()
    {
        await using var connection = new NpgsqlConnection(_factory.GetConnectionString());
        await connection.OpenAsync();

        // Create test addresses
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO ""Addresses"" (""Id"", ""Street1"", ""City"", ""State"", ""PostalCode"", ""CountryCode"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted"")
            VALUES
                ('11111111-1111-1111-1111-111111111111', '123 Shipper St', 'Almaty', 'Almaty', '050000', 'KZ', NOW(), 'test-seeder', false),
                ('22222222-2222-2222-2222-222222222222', '456 Recipient Ave', 'Almaty', 'Almaty', '050000', 'KZ', NOW(), 'test-seeder', false)
            ON CONFLICT (""Id"") DO NOTHING;", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Create test parcels with different statuses
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO ""Parcel"" (
                ""Id"", ""TrackingNumber"", ""Status"", ""Description"", ""ServiceType"",
                ""Weight"", ""WeightUnit"", ""Length"", ""Width"", ""Height"", ""DimensionUnit"",
                ""ShipperAddressId"", ""RecipientAddressId"", ""DeclaredValue"", ""Currency"",
                ""DeliveryAttempts"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted""
            )
            VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'LMTT1-0403-0001', 'Registered', 'Test Parcel - Registered', 'Standard', 1.5, 'Kg', 30, 20, 15, 'Cm', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', 100.00, 'USD', 0, NOW(), 'test-seeder', false),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'LMTT1-0403-0002', 'Sorted', 'Test Parcel - Sorted', 'Express', 2.0, 'Kg', 40, 30, 20, 'Cm', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', 150.00, 'USD', 0, NOW(), 'test-seeder', false),
                ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'LMTT1-0403-0003', 'Loaded', 'Test Parcel - Loaded', 'Standard', 1.8, 'Kg', 35, 25, 18, 'Cm', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', 120.00, 'USD', 0, NOW(), 'test-seeder', false)
            ON CONFLICT (""Id"") DO NOTHING;", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }

    #region UpdateParcel Tests

    [Fact]
    public async Task UpdateParcel_ShipperAddress_ShouldCreateNewAddressAndLinkToParcel()
    {
        // Arrange
        var mutation = @"mutation {
            updateParcel(input: {
                id: ""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"",
                shipperAddress: {
                    street1: ""789 New Shipper Blvd"",
                    street2: ""Apt 5"",
                    city: ""Almaty"",
                    state: ""Almaty"",
                    postalCode: ""050001"",
                    countryCode: ""KZ"",
                    isResidential: false,
                    contactName: ""New Shipper Contact"",
                    companyName: ""New Shipper Co"",
                    phone: ""+77771234567"",
                    email: ""newshipper@example.com""
                }
            }) {
                id
                shipperAddress {
                    id
                    street1
                    city
                    postalCode
                }
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update failed: {errors.GetRawText()}");
        }

        var result = jsonResponse.GetProperty("data").GetProperty("updateParcel");
        var address = result.GetProperty("shipperAddress");
        address.GetProperty("street1").GetString().Should().Be("789 New Shipper Blvd");
        address.GetProperty("city").GetString().Should().Be("Almaty");
        address.GetProperty("postalCode").GetString().Should().Be("050001");
    }

    [Fact]
    public async Task UpdateParcel_RecipientAddress_ShouldCreateNewAddressAndLinkToParcel()
    {
        // Arrange
        var mutation = @"mutation {
            updateParcel(input: {
                id: ""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"",
                recipientAddress: {
                    street1: ""999 New Recipient Way"",
                    city: ""Almaty"",
                    state: ""Almaty"",
                    postalCode: ""050002"",
                    countryCode: ""KZ"",
                    isResidential: true,
                    contactName: ""New Recipient Person"",
                    phone: ""+77779876543""
                }
            }) {
                id
                recipientAddress {
                    id
                    street1
                    contactName
                    postalCode
                }
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update failed: {errors.GetRawText()}");
        }

        var result = jsonResponse.GetProperty("data").GetProperty("updateParcel");
        var address = result.GetProperty("recipientAddress");
        address.GetProperty("street1").GetString().Should().Be("999 New Recipient Way");
        address.GetProperty("contactName").GetString().Should().Be("New Recipient Person");
    }

    [Fact]
    public async Task UpdateParcel_WithValidData_OnRegisteredParcel_ShouldUpdateAndLogChanges()
    {
        // Arrange
        var mutation = @"mutation {
            updateParcel(input: {
                id: ""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"",
                description: ""UPDATED - Registered Parcel"",
                weight: 2.5,
                length: 35,
                width: 25,
                height: 20,
                parcelType: ""Fragile""
            }) {
                id
                trackingNumber
                status
                description
                weight
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Update failed: {errors.GetRawText()}");
        }

        var result = jsonResponse.GetProperty("data").GetProperty("updateParcel");
        result.GetProperty("description").GetString().Should().Be("UPDATED - Registered Parcel");
        result.GetProperty("weight").GetDecimal().Should().Be(2.5m);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Sorted")]  // Allowed
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc", "Loaded")]   // Not allowed
    public async Task UpdateParcel_Validation_ShouldOnlyAllowEditableStatuses(string parcelId, string expectedStatus)
    {
        // Arrange
        var mutation = $@"mutation {{
            updateParcel(input: {{
                id: ""{parcelId}"",
                description: ""Should this work?"",
                weight: 2.5,
                length: 35,
                width: 25,
                height: 20,
                parcelType: ""Fragile""
            }}) {{
                id
                description
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        if (expectedStatus == "Sorted")
        {
            // Should succeed for Sorted
            jsonResponse.TryGetProperty("errors", out var errors).Should().BeFalse("Update should succeed for Sorted status");
        }
        else // Loaded
        {
            // Should fail for Loaded
            jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue("Update should fail for Loaded status");
            errors[0].GetProperty("message").GetString().Should().Contain("Cannot edit parcel in status");
        }
    }

    [Fact]
    public async Task UpdateParcel_WithInvalidWeight_ShouldReturnValidationError()
    {
        // Arrange
        var mutation = @"mutation {
            updateParcel(input: {
                id: ""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"",
                weight: -1.5,
                length: 35,
                width: 25,
                height: 20,
                serviceType: STANDARD
            }) {
                id
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("Weight");
    }

    #endregion

    #region CancelParcel Tests

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Registered")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Sorted")]
    public async Task CancelParcel_WhenInAllowedStatus_ShouldCancel(string parcelId, string status)
    {
        // Arrange
        var mutation = $@"mutation {{
            cancelParcel(input: {{
                id: ""{parcelId}"",
                reason: ""Customer requested cancellation""
            }}) {{
                id
                trackingNumber
                status
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Cancel failed for {status}: {errors.GetRawText()}");
        }

        var result = jsonResponse.GetProperty("data").GetProperty("cancelParcel");
        result.GetProperty("status").GetString().Should().Be("CANCELLED");
    }

    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc", "Loaded")]
    public async Task CancelParcel_WhenInDisallowedStatus_ShouldReturnError(string parcelId, string status)
    {
        // Arrange
        var mutation = $@"mutation {{
            cancelParcel(input: {{
                id: ""{parcelId}"",
                reason: ""Test cancellation""
            }}) {{
                id
                status
            }}
        }}";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue($"Cancel should fail for {status} status");
        errors[0].GetProperty("message").GetString().Should().Contain("Cannot cancel parcel");
    }

    [Fact]
    public async Task CancelParcel_WithoutReason_ShouldReturnValidationError()
    {
        // Arrange
        var mutation = @"mutation {
            cancelParcel(input: {
                id: ""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa""
            }) {
                id
            }
        }";

        // Act
        var jsonResponse = await GraphQLRequestAsync(mutation);

        // Assert
        jsonResponse.TryGetProperty("errors", out var errors).Should().BeTrue();
        var errorMessage = errors[0].GetProperty("message").GetString();
        errorMessage.Should().Match("*eason*"); // Matches "Reason" or "reason"
    }

    [Fact]
    public async Task CancelParcel_ShouldCreateAuditLogWithReason()
    {
        // Arrange
        var parcelId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        var reason = "Customer requested cancellation";
        var mutation = $@"mutation {{
            cancelParcel(input: {{
                id: ""{parcelId}"",
                reason: ""{reason}""
            }}) {{
                id
                status
            }}
        }}";

        // Act - Cancel the parcel
        await GraphQLRequestAsync(mutation);

        // Query audit logs
        var query = $@"query {{
            parcelAuditLogs(parcelId: ""{parcelId}"") {{
                nodes {{
                    propertyName
                    oldValue
                    newValue
                    changedBy
                    createdAt
                }}
            }}
        }}";

        var jsonResponse = await GraphQLRequestAsync(query);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Query failed: {errors.GetRawText()}");
        }

        var auditLogs = jsonResponse.GetProperty("data").GetProperty("parcelAuditLogs").GetProperty("nodes");
        auditLogs.GetArrayLength().Should().BeGreaterThan(0);

        // Find the status change audit log
        var statusAuditLog = auditLogs.EnumerateArray()
            .FirstOrDefault(log => log.GetProperty("propertyName").GetString() == "Status");

        statusAuditLog.Should().NotBeNull();
        statusAuditLog.GetProperty("oldValue").GetString().Should().Be("Registered");
        statusAuditLog.GetProperty("newValue").GetString().Should().Be($"Cancelled - {reason}");
        statusAuditLog.GetProperty("changedBy").GetString().Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Audit Log Tests

    [Fact]
    public async Task UpdateParcel_ShouldCreateAuditLogEntries()
    {
        // Arrange
        var parcelId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        var mutation = $@"mutation {{
            updateParcel(input: {{
                id: ""{parcelId}"",
                description: ""AUDIT TEST - Updated Description"",
                weight: 3.5,
                length: 40,
                width: 30,
                height: 25,
                serviceType: EXPRESS,
                parcelType: ""Electronics""
            }}) {{
                id
            }}
        }}";

        // Act - Update the parcel
        await GraphQLRequestAsync(mutation);

        // Query audit logs
        var query = $@"query {{
            parcelAuditLogs(parcelId: ""{parcelId}"") {{
                nodes {{
                    propertyName
                    oldValue
                    newValue
                    changedBy
                    createdAt
                }}
            }}
        }}";

        var jsonResponse = await GraphQLRequestAsync(query);

        // Assert
        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"Query failed: {errors.GetRawText()}");
        }

        var auditLogs = jsonResponse.GetProperty("data").GetProperty("parcelAuditLogs").GetProperty("nodes");
        auditLogs.GetArrayLength().Should().BeGreaterThan(0);

        // Verify specific changes were logged
        var loggedProperties = new HashSet<string>();
        foreach (var log in auditLogs.EnumerateArray())
        {
            loggedProperties.Add(log.GetProperty("propertyName").GetString()!);
        }

        loggedProperties.Should().Contain("Description");
        loggedProperties.Should().Contain("Weight");
        loggedProperties.Should().Contain("ServiceType");
    }

    #endregion

    private static async Task CleanupTestDataAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Clean up in correct order due to FK constraints
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"ParcelAuditLogs\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Parcel\";", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("DELETE FROM \"Addresses\" WHERE \"Id\" IN ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222');", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
