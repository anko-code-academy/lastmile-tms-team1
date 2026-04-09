using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace LastMile.TMS.Api.IntegrationTests;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:17-3.5")
        .WithDatabase("lastmile_tms_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public string AccessToken { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        // Enable PostGIS extension
        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS postgis;", connection);
        await command.ExecuteNonQueryAsync();

        Factory = new IntegrationTestWebApplicationFactory(_postgreSqlContainer.GetConnectionString());
        await Factory.InitializeAsync();

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        var dbSeeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
        await dbSeeder.SeedAsync();

        var username = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

        var tokenResponse = await Client.PostAsync("/connect/token",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            }));

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenContent);

        if (!tokenResponse.IsSuccessStatusCode || !tokenJson.TryGetProperty("access_token", out _))
        {
            throw new Exception($"Token request failed: {tokenContent}");
        }

        AccessToken = tokenJson.GetProperty("access_token").GetString()!;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task<JsonElement> GraphQLRequestAsync(string query, object? variables = null)
    {
        var requestBody = new { query, variables };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await Client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseContent);
    }

    public async Task<HttpResponseMessage> ExecuteGraphQLAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        return await Client.SendAsync(request);
    }

    public static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonDocument>(content)!;
    }

    public IServiceScope CreateScope() => Factory.Services.CreateScope();
}
