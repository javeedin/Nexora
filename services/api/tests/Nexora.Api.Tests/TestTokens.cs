using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Nexora.Api.Tests;

/// <summary>A test user: tenants (alias → id), roles, MFA.</summary>
public sealed record TestUser(string Username, IReadOnlyDictionary<string, string> Tenants, string[] Roles, bool Mfa = false)
{
    public string Sub { get; } = Guid.NewGuid().ToString();

    public static readonly TestUser AcmeAdmin = new("admin@acme.test", new Dictionary<string, string> { ["acme"] = TestTokens.AcmeId }, ["tenant-admin", "user"]);
    public static readonly TestUser AcmeUser = new("user@acme.test", new Dictionary<string, string> { ["acme"] = TestTokens.AcmeId }, ["user"]);
    public static readonly TestUser GlobexUser = new("user@globex.test", new Dictionary<string, string> { ["globex"] = TestTokens.GlobexId }, ["user"]);
    public static readonly TestUser Consultant = new("consultant@partner.test",
        new Dictionary<string, string> { ["acme"] = TestTokens.AcmeId, ["globex"] = TestTokens.GlobexId }, ["user"]);
    public static readonly TestUser Vendor = new("vendor@nexora.test", new Dictionary<string, string>(), ["platform-admin"]);

    public TestUser WithMfa() => this with { Mfa = true };
}

/// <summary>Mints tokens shaped like Keycloak's (realm_access, tenants, acr), signed with a test key.</summary>
public static class TestTokens
{
    public const string Issuer = "https://idp.test/realms/nexora";
    public const string Audience = "nexora-api";
    public const string AcmeId = "11111111-1111-4111-8111-111111111111";
    public const string GlobexId = "22222222-2222-4222-8222-222222222222";

    public static readonly SymmetricSecurityKey Key = new(Encoding.UTF8.GetBytes("nexora-test-signing-key-only-for-tests-0123456789"));

    public static string For(TestUser user, string issuer = Issuer, string audience = Audience, DateTime? expires = null, SecurityKey? key = null)
    {
        var tenants = user.Tenants.ToDictionary(t => t.Key, t => (object)new Dictionary<string, object> { ["id"] = t.Value });
        var claims = new Dictionary<string, object>
        {
            ["sub"] = user.Sub,
            ["preferred_username"] = user.Username,
            ["email"] = user.Username,
            ["name"] = user.Username,
            ["realm_access"] = new Dictionary<string, object> { ["roles"] = user.Roles },
            ["acr"] = user.Mfa ? "mfa" : "pwd",
        };
        if (tenants.Count > 0)
        {
            claims["tenants"] = tenants;
        }

        var now = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = claims,
            IssuedAt = now.AddMinutes(-2),
            NotBefore = now.AddMinutes(-2),
            Expires = expires ?? now.AddMinutes(10),
            SigningCredentials = new SigningCredentials(key ?? Key, SecurityAlgorithms.HmacSha256),
        });
    }

    public static ClaimsIdentity Empty => new();
}
