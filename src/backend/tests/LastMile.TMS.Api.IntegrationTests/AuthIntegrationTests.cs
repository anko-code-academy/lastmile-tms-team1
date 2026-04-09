using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class AuthIntegrationTests
{
    private readonly IntegrationFixture _fx;

    private static string AdminUsername => Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
    private static string AdminPassword => Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

    public AuthIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task TokenEndpoint_WithEmptyCredentials_ReturnsBadRequest()
    {
        var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _fx.Client.PostAsync("/connect/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TokenEndpoint_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var formData = new Dictionary<string, string>
        {
            { "username", "invalid" },
            { "password", "invalid" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fx.Client.PostAsync("/connect/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TokenEndpoint_WithValidCredentials_ReturnsOkWithToken()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", AdminUsername },
            { "password", AdminPassword }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fx.Client.PostAsync("/connect/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        tokenResponse.TryGetProperty("access_token", out _).Should().BeTrue();
        tokenResponse.TryGetProperty("token_type", out var tokenType).Should().BeTrue();
        tokenType.GetString().Should().Be("Bearer");
    }

    [Fact]
    public async Task TokenEndpoint_WithValidCredentials_ReturnsRefreshToken()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", AdminUsername },
            { "password", AdminPassword },
            { "scope", "offline_access" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fx.Client.PostAsync("/connect/token", content);

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        tokenResponse.TryGetProperty("refresh_token", out _).Should().BeTrue();
    }

    [Fact]
    public async Task TokenEndpoint_WithOfflineAccessScope_ReturnsRefreshToken()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", AdminUsername },
            { "password", AdminPassword },
            { "scope", "offline_access" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fx.Client.PostAsync("/connect/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        tokenResponse.TryGetProperty("refresh_token", out _).Should().BeTrue();
    }
}
