using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Nexora.Api.Tests;

public sealed class SecurityTests(TestApi api) : IClassFixture<TestApi>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<JsonElement> Json(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(Ct);

    [Fact]
    public async Task Anonymous_call_to_a_protected_endpoint_is_401_problem_with_bearer_challenge()
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Public_endpoints_stay_anonymous()
    {
        using var client = api.CreateClient();
        foreach (var path in new[] { "/health/live", "/health/ready", "/api/v1/platform/info", "/openapi/v1.json" })
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(new Uri(path, UriKind.Relative), Ct)).StatusCode);
        }
    }

    [Fact]
    public async Task Me_shows_tenant_and_roles_from_the_token()
    {
        using var client = api.ClientFor(TestUser.AcmeAdmin);
        var me = await Json(await client.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct));
        Assert.Equal(TestTokens.AcmeId, me.GetProperty("tenant").GetProperty("id").GetString());
        Assert.Equal("acme", me.GetProperty("tenant").GetProperty("alias").GetString());
        Assert.Contains("tenant-admin", me.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));
        Assert.False(me.GetProperty("mfa").GetBoolean());
    }

    [Theory]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("expired")]
    [InlineData("signature")]
    public async Task Invalid_tokens_are_rejected(string defect)
    {
        var token = defect switch
        {
            "issuer" => TestTokens.For(TestUser.AcmeAdmin, issuer: "https://evil.test/realms/nexora"),
            "audience" => TestTokens.For(TestUser.AcmeAdmin, audience: "account"),
            "expired" => TestTokens.For(TestUser.AcmeAdmin, expires: DateTime.UtcNow.AddMinutes(-10)),
            _ => TestTokens.For(TestUser.AcmeAdmin, key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes("another-key-another-key-another-key-0123456789"))),
        };
        using var client = api.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct)).StatusCode);
    }

    [Fact]
    public async Task User_in_several_tenants_must_choose_one_and_can_choose_by_alias_or_id()
    {
        using (var none = api.ClientFor(TestUser.Consultant))
        {
            var me = await Json(await none.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct));
            Assert.Equal(JsonValueKind.Null, me.GetProperty("tenant").ValueKind);
            Assert.Equal(2, me.GetProperty("tenants").GetArrayLength());
            var write = await none.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, Guid.NewGuid().ToString()), Ct);
            Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
            Assert.Contains("X-Nexora-Tenant", (await Json(write)).GetProperty("detail").GetString(), StringComparison.Ordinal);
        }

        using var byAlias = api.ClientFor(TestUser.Consultant, tenant: "globex");
        Assert.Equal(TestTokens.GlobexId, (await Json(await byAlias.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct))).GetProperty("tenant").GetProperty("id").GetString());
        using var byId = api.ClientFor(TestUser.Consultant, tenant: TestTokens.AcmeId);
        Assert.Equal("acme", (await Json(await byId.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct))).GetProperty("tenant").GetProperty("alias").GetString());
    }

    [Fact]
    public async Task Asking_for_a_tenant_you_do_not_belong_to_is_403_and_never_falls_back()
    {
        using var client = api.ClientFor(TestUser.AcmeUser, tenant: "globex");
        var me = await Json(await client.GetAsync(new Uri("/api/v1/platform/me", UriKind.Relative), Ct));
        Assert.Equal(JsonValueKind.Null, me.GetProperty("tenant").ValueKind); // not silently acme

        var write = await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, Guid.NewGuid().ToString()), Ct);
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
        Assert.Contains("not a member of tenant 'globex'", (await Json(write)).GetProperty("detail").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Platform_staff_without_tenant_cannot_use_tenant_endpoints()
    {
        using var client = api.ClientFor(TestUser.Vendor.WithMfa());
        var write = await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, Guid.NewGuid().ToString()), Ct);
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Fact]
    public async Task Regular_user_cannot_invite()
    {
        using var client = api.ClientFor(TestUser.AcmeUser.WithMfa());
        var response = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email = "a@acme.test" }, Guid.NewGuid().ToString()), Ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain(response.Headers.WwwAuthenticate, h => h.Parameter?.Contains("insufficient_user_authentication", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task Admin_without_second_factor_gets_the_rfc9470_step_up_challenge()
    {
        using var client = api.ClientFor(TestUser.AcmeAdmin);
        var response = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email = "a@acme.test" }, Guid.NewGuid().ToString()), Ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var challenge = response.Headers.WwwAuthenticate.Single();
        Assert.Contains("error=\"insufficient_user_authentication\"", challenge.Parameter, StringComparison.Ordinal);
        Assert.Contains("acr_values=\"mfa\"", challenge.Parameter, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Admin_with_mfa_invites_into_their_own_tenant_once_per_idempotency_key()
    {
        using var client = api.ClientFor(TestUser.AcmeAdmin.WithMfa());
        var key = Guid.NewGuid().ToString();
        var email = $"{Guid.NewGuid():N}@acme.test";
        var first = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email }, key), Ct);
        var replay = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email }, key), Ct);

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, replay.StatusCode);
        Assert.Equal(TestTokens.AcmeId, (await Json(first)).GetProperty("tenantId").GetString());
        Assert.Single(api.Identity.Invitations, i => i.Email == email && i.TenantId == TestTokens.AcmeId);
    }

    [Fact]
    public async Task Inviting_an_existing_member_is_409_and_bad_email_is_400()
    {
        using var client = api.ClientFor(TestUser.AcmeAdmin.WithMfa());
        api.Identity.ExistingMembers.Add("already@acme.test");
        var conflict = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email = "already@acme.test" }, Guid.NewGuid().ToString()), Ct);
        var invalid = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email = "not-an-email" }, Guid.NewGuid().ToString()), Ct);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Idempotency_keys_are_scoped_per_tenant()
    {
        var key = Guid.NewGuid().ToString();
        var before = api.Module.Executions;
        using var acme = api.ClientFor(TestUser.AcmeUser);
        using var globex = api.ClientFor(TestUser.GlobexUser);
        var a = await acme.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, key), Ct);
        var b = await globex.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "b", quantity = 2 }, key), Ct);
        Assert.Equal(HttpStatusCode.Created, a.StatusCode);
        Assert.Equal(HttpStatusCode.Created, b.StatusCode); // not 422: another tenant's key space
        Assert.Equal(before + 2, api.Module.Executions);
    }
}

public sealed class TenantRateLimitTests
{
    [Fact]
    public async Task Each_tenant_has_its_own_bucket()
    {
        await using var api = new TestApi { Settings = new() { ["RateLimiting:PermitsPerMinute"] = "2" } };
        var ct = TestContext.Current.CancellationToken;
        using var acme = api.ClientFor(TestUser.AcmeUser);
        using var globex = api.ClientFor(TestUser.GlobexUser);
        var me = new Uri("/api/v1/platform/me", UriKind.Relative);

        Assert.Equal(HttpStatusCode.OK, (await acme.GetAsync(me, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await acme.GetAsync(me, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await acme.GetAsync(me, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await globex.GetAsync(me, ct)).StatusCode);
    }
}

public sealed class MfaConfigurationTests
{
    [Fact]
    public async Task Mfa_requirement_can_be_switched_off_for_local_development()
    {
        await using var api = new TestApi { Settings = new() { ["Auth:RequireMfaForAdmins"] = "false" } };
        using var client = api.ClientFor(TestUser.AcmeAdmin);
        var response = await client.SendAsync(TestApi.Post("/api/v1/platform/invitations", new { email = "dev@acme.test" }, Guid.NewGuid().ToString()), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }
}
