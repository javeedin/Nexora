namespace Nexora.BuildingBlocks.Security;

/// <summary>Claim types Nexora derives from the identity provider's token (ADR 0010).</summary>
public static class NexoraClaims
{
    /// <summary>Selected tenant id (Keycloak organisation id, UUID).</summary>
    public const string TenantId = "nexora:tenant_id";

    /// <summary>Selected tenant alias (organisation alias, e.g. <c>acme</c>).</summary>
    public const string TenantAlias = "nexora:tenant";

    /// <summary>A tenant membership, value <c>alias|id</c>; one claim per organisation of the user.</summary>
    public const string Membership = "nexora:membership";

    /// <summary>Set when the requested tenant (<see cref="TenantHeader"/>) is not one of the user's.</summary>
    public const string TenantRejected = "nexora:tenant_rejected";

    /// <summary>Role claim type (from <c>realm_access.roles</c>).</summary>
    public const string Role = "role";

    /// <summary>Header that picks the tenant for users who belong to several.</summary>
    public const string TenantHeader = "X-Nexora-Tenant";
}

/// <summary>Role names (realm roles in Keycloak).</summary>
public static class NexoraRoles
{
    /// <summary>Our staff: vendor console (PC-14).</summary>
    public const string PlatformAdmin = "platform-admin";

    /// <summary>Administers one tenant (PC-13).</summary>
    public const string TenantAdmin = "tenant-admin";

    /// <summary>Regular tenant user.</summary>
    public const string User = "user";
}

/// <summary>Authorization policy names.</summary>
public static class NexoraPolicies
{
    /// <summary>Signed in and acting for exactly one tenant they belong to.</summary>
    public const string TenantMember = "tenant-member";

    /// <summary>Tenant member with the tenant-admin role (+ MFA when required).</summary>
    public const string TenantAdmin = "tenant-admin";

    /// <summary>Nexora staff (+ MFA when required).</summary>
    public const string PlatformAdmin = "platform-admin";
}
