using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LastMile.TMS.Api.Tests;

public class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Use in-memory database for testing
        builder.ConfigureServices(services =>
        {
            // Configure test-specific services if needed
        });

        return base.CreateHost(builder);
    }
}

/// <summary>
/// Integration tests for authentication endpoints. These tests require Docker to be running
/// with PostgreSQL and Redis containers.
/// </summary>
[IntegrationTest]
public class AuthIntegrationTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly AuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [IntegrationTestFact]
    public async Task TokenEndpoint_WithEmptyCredentials_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        // Act
        var response = await _client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [IntegrationTestFact]
    public async Task TokenEndpoint_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "username", "invalid" },
            { "password", "invalid" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [IntegrationTestFact]
    public async Task TokenEndpoint_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "admin" },
            { "password", "Admin@123" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        tokenResponse.TryGetProperty("access_token", out _).Should().BeTrue();
        tokenResponse.TryGetProperty("token_type", out var tokenType).Should().BeTrue();
        tokenType.GetString().Should().Be("Bearer");
    }

    [IntegrationTestFact]
    public async Task TokenEndpoint_WithValidCredentials_ReturnsRefreshToken()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "admin" },
            { "password", "Admin@123" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/connect/token", content);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        tokenResponse.TryGetProperty("refresh_token", out _).Should().BeTrue();
    }

    [IntegrationTestFact]
    public async Task TokenEndpoint_WithOfflineAccessScope_ReturnsRefreshToken()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "admin" },
            { "password", "Admin@123" },
            { "scope", "offline_access" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        tokenResponse.TryGetProperty("refresh_token", out _).Should().BeTrue();
    }
}

// Custom fact attribute to skip integration tests unless explicitly enabled
[AttributeUsage(AttributeTargets.Method)]
public class IntegrationTestFactAttribute : FactAttribute
{
    public IntegrationTestFactAttribute()
    {
        if (!IsIntegrationTestEnabled())
        {
            Skip = "Integration tests require Docker to be running. Run 'docker compose up' to enable.";
        }
    }

    private static bool IsIntegrationTestEnabled()
    {
        return Environment.GetEnvironmentVariable("ENABLE_INTEGRATION_TESTS") == "1";
    }
}

// Custom trait attribute for categorizing integration tests
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class IntegrationTestAttribute : Attribute { }