using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.Keycloak;

namespace Nexora.Api.Tests;

/// <summary>
/// End to end with the real realm file (infra/compose/keycloak/nexora-realm.json) in a real Keycloak: a password
/// login yields a token the API accepts, and the tenant is the organisation's id. Runs in CI (Docker available).
/// </summary>
public sealed class KeycloakIntegrationTests : IAsyncLifetime
{
    private const string Password = "integration-Password-123";
    private readonly KeycloakContainer _keycloak = new KeycloakBuilder("keycloak/keycloak:26.8.0")
        .WithResourceMapping(new FileInfo(RealmFile()), "/opt/keycloak/data/import/")
        .WithEnvironment("NEXORA_DEV_USER_PASSWORD", Password)
        .WithEnvironment("NEXORA_API_CLIENT_SECRET", "integration-secret")
        .WithCommand("--import-realm")
        .Build();

    public async ValueTask InitializeAsync() => await _keycloak.StartAsync();

    public async ValueTask DisposeAsync() => await _keycloak.DisposeAsync();

    [Fact]
    public async Task Real_keycloak_token_signs_the_user_in_with_tenant_and_roles()
    {
        var ct = TestContext.Current.CancellationToken;
        var baseUrl = _keycloak.GetBaseAddress().TrimEnd('/');
        using var http = new HttpClient();
        var token = await http.PostAsync(new Uri($"{baseUrl}/realms/nexora/protocol/openid-connect/token"), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "nexora-dev-cli",
            ["username"] = "admin@acme.test",
            ["password"] = Password,
            ["scope"] = "openid",
        }), ct);
        token.EnsureSuccessStatusCode();
        var accessToken = (await token.Content.ReadFromJsonAsync<JsonElement>(ct)).GetProperty("access_token").GetString();

        await using var api = new TestApi { RealAuthority = $"{baseUrl}/realms/nexora" };
        using var client = api.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var me = await client.GetFromJsonAsync<JsonElement>("/api/v1/platform/me", ct);

        Assert.Equal("acme", me.GetProperty("tenant").GetProperty("alias").GetString());
        Assert.True(Guid.TryParse(me.GetProperty("tenant").GetProperty("id").GetString(), out _));
        Assert.Contains("tenant-admin", me.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));
        Assert.False(string.IsNullOrEmpty(me.GetProperty("userId").GetString()));
    }

    private static string RealmFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "infra/compose/keycloak/nexora-realm.json")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir?.FullName ?? throw new InvalidOperationException("repo root not found"), "infra/compose/keycloak/nexora-realm.json");
    }
}
