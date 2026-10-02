using System.Net;
using Microsoft.Extensions.Options;
using Nexora.Modules.Platform.Identity;

namespace Nexora.Api.Tests;

public sealed class KeycloakIdentityAdminTests
{
    private sealed class Recorder(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(string Path, string Body, string? Auth)> Calls { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls.Add((request.RequestUri!.AbsolutePath, request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken), request.Headers.Authorization?.Parameter));
            return respond(request);
        }
    }

    private static (KeycloakIdentityAdmin Admin, Recorder Handler) Create(HttpStatusCode inviteStatus = HttpStatusCode.NoContent)
    {
        var handler = new Recorder(r => r.RequestUri!.AbsolutePath.EndsWith("/token", StringComparison.Ordinal)
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"access_token\":\"svc-token\",\"expires_in\":300}", System.Text.Encoding.UTF8, "application/json") }
            : new HttpResponseMessage(inviteStatus));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://kc.test/") };
        var options = Options.Create(new KeycloakOptions { Realm = "nexora", ClientId = "nexora-api", ClientSecret = "s3cret" });
        return (new KeycloakIdentityAdmin(http, options, new KeycloakTokenCache(), TimeProvider.System), handler);
    }

    [Fact]
    public async Task Invites_through_the_organisation_endpoint_with_a_cached_service_token()
    {
        var (admin, handler) = Create();
        var ct = TestContext.Current.CancellationToken;
        await admin.InviteToTenantAsync("org-1", "a@acme.test", "Ann", null, ct);
        await admin.InviteToTenantAsync("org-1", "b@acme.test", null, null, ct);

        Assert.Single(handler.Calls, c => c.Path == "/realms/nexora/protocol/openid-connect/token");
        var invites = handler.Calls.Where(c => c.Path == "/admin/realms/nexora/organizations/org-1/members/invite-user").ToList();
        Assert.Equal(2, invites.Count);
        Assert.All(invites, c => Assert.Equal("svc-token", c.Auth));
        Assert.Contains("email=a%40acme.test", invites[0].Body, StringComparison.Ordinal);
        Assert.Contains("firstName=Ann", invites[0].Body, StringComparison.Ordinal);
        Assert.DoesNotContain("lastName", invites[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Conflict_from_keycloak_becomes_IdentityConflictException()
    {
        var (admin, _) = Create(HttpStatusCode.Conflict);
        await Assert.ThrowsAsync<IdentityConflictException>(() =>
            admin.InviteToTenantAsync("org-1", "a@acme.test", null, null, TestContext.Current.CancellationToken));
    }
}
