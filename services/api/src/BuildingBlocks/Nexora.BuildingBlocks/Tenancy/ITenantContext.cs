namespace Nexora.BuildingBlocks.Tenancy;

/// <summary>
/// The tenant of the current request (rule 1). Resolved from the token in P0-T07; until then no tenant is known
/// and tenant-scoped keys use <see cref="PlatformScope"/>.
/// </summary>
public interface ITenantContext
{
    /// <summary>Scope used when no tenant applies (anonymous / platform-level calls).</summary>
    public const string PlatformScope = "_platform";

    /// <summary>Tenant id of the current request, or <c>null</c> when none is resolved.</summary>
    string? TenantId { get; }

    /// <summary>Tenant id, or <see cref="PlatformScope"/> — for cache keys, rate-limit partitions, file paths.</summary>
    string Scope => TenantId ?? PlatformScope;
}

/// <summary>Placeholder until P0-T07: no tenant resolved.</summary>
public sealed class NoTenantContext : ITenantContext
{
    /// <inheritdoc />
    public string? TenantId => null;
}
