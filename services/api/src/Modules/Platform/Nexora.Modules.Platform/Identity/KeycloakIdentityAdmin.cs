using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Nexora.Modules.Platform.Identity;

/// <summary>Configuration section <c>Keycloak</c>. <see cref="ClientSecret"/> comes from the environment / Vault (P0-T13), never a file.</summary>
public sealed class KeycloakOptions
{
    /// <summary>Base URL, e.g. <c>https://id.example.com</c>.</summary>
    public string BaseUrl { get; set; } = "http://localhost:8180";

    /// <summary>Realm.</summary>
    public string Realm { get; set; } = "nexora";

    /// <summary>Service-account client of the API.</summary>
    public string ClientId { get; set; } = "nexora-api";

    /// <summary>Client secret of <see cref="ClientId"/>.</summary>
    public string? ClientSecret { get; set; }
}

/// <summary>Keycloak admin REST client using the API's service account (client credentials, token cached).</summary>
public sealed class KeycloakIdentityAdmin(HttpClient http, IOptions<KeycloakOptions> options, KeycloakTokenCache tokens, TimeProvider time) : IIdentityAdmin
{
    private readonly KeycloakOptions _options = options.Value;

    /// <inheritdoc />
    public async Task InviteToTenantAsync(string tenantId, string email, string? firstName, string? lastName, CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string> { ["email"] = email };
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            form["firstName"] = firstName;
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            form["lastName"] = lastName;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations/{Uri.EscapeDataString(tenantId)}/members/invite-user")
        {
            Content = new FormUrlEncodedContent(form),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessTokenAsync(cancellationToken));
        using var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new IdentityConflictException($"{email} is already a member of this tenant.");
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> AccessTokenAsync(CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        if (tokens.Get(now) is { } cached)
        {
            return cached;
        }

        using var response = await http.PostAsync(
            $"realms/{Uri.EscapeDataString(_options.Realm)}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret ?? throw new InvalidOperationException("Keycloak:ClientSecret is not configured."),
            }),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Empty token response from Keycloak.");
        tokens.Set(token.AccessToken, now.AddSeconds(Math.Max(0, token.ExpiresIn - 30)));
        return token.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}

/// <summary>Holds the service-account token until shortly before it expires (singleton).</summary>
public sealed class KeycloakTokenCache
{
    private readonly Lock _gate = new();
    private (string Token, DateTimeOffset Until)? _entry;

    /// <summary>The cached token if still valid at <paramref name="now"/>.</summary>
    public string? Get(DateTimeOffset now)
    {
        lock (_gate)
        {
            return _entry is { } e && e.Until > now ? e.Token : null;
        }
    }

    /// <summary>Caches <paramref name="token"/> until <paramref name="until"/>.</summary>
    public void Set(string token, DateTimeOffset until)
    {
        lock (_gate)
        {
            _entry = (token, until);
        }
    }
}
