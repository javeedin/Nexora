using System.Security.Claims;
using System.Text.Json;

namespace Nexora.BuildingBlocks.Security;

/// <summary>Turns the IdP token's JSON claims into Nexora claims: roles, tenant memberships, selected tenant.</summary>
public static class TokenClaims
{
    /// <summary>
    /// Adds role, membership and selected-tenant claims. The tenant is the requested one (<paramref name="requestedTenant"/>,
    /// alias or id) if the user belongs to it, else the only membership; with several and none requested, no tenant.
    /// </summary>
    public static void Enrich(ClaimsIdentity identity, string? requestedTenant)
    {
        ArgumentNullException.ThrowIfNull(identity);
        foreach (var role in ReadRoles(identity))
        {
            identity.AddClaim(new Claim(NexoraClaims.Role, role));
        }

        var memberships = ReadMemberships(identity);
        foreach (var (alias, id) in memberships)
        {
            identity.AddClaim(new Claim(NexoraClaims.Membership, $"{alias}|{id}"));
        }

        (string Alias, string Id)? selected = null;
        if (!string.IsNullOrWhiteSpace(requestedTenant))
        {
            selected = memberships.FirstOrDefault(m => m.Alias == requestedTenant || m.Id == requestedTenant) is { Id: not null } hit ? hit : null;
            if (selected is null)
            {
                identity.AddClaim(new Claim(NexoraClaims.TenantRejected, requestedTenant));
            }
        }
        else if (memberships.Count == 1)
        {
            selected = memberships[0];
        }

        if (selected is { } tenant)
        {
            identity.AddClaim(new Claim(NexoraClaims.TenantId, tenant.Id));
            identity.AddClaim(new Claim(NexoraClaims.TenantAlias, tenant.Alias));
        }
    }

    private static IEnumerable<string> ReadRoles(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (realmAccess is null)
        {
            return [];
        }

        using var doc = JsonDocument.Parse(realmAccess);
        return doc.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array
            ? [.. roles.EnumerateArray().Select(r => r.GetString()).OfType<string>()]
            : [];
    }

    private static List<(string Alias, string Id)> ReadMemberships(ClaimsIdentity identity)
    {
        var tenants = identity.FindFirst("tenants")?.Value;
        if (tenants is null)
        {
            return [];
        }

        using var doc = JsonDocument.Parse(tenants);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        return [.. doc.RootElement.EnumerateObject()
            .Where(o => o.Value.ValueKind == JsonValueKind.Object && o.Value.TryGetProperty("id", out _))
            .Select(o => (o.Name, o.Value.GetProperty("id").GetString()!))
            .OrderBy(m => m.Name, StringComparer.Ordinal)];
    }
}
