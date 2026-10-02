using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nexora.BuildingBlocks.Tenancy;

namespace Nexora.BuildingBlocks.Security;

/// <summary>A tenant the user belongs to.</summary>
/// <param name="Id">Tenant id (organisation id).</param>
/// <param name="Alias">Tenant alias.</param>
public sealed record TenantRef(string Id, string Alias);

/// <summary>The signed-in user of the current request.</summary>
public interface ICurrentUser
{
    /// <summary>Signed in?</summary>
    bool IsAuthenticated { get; }

    /// <summary>Stable user id (<c>sub</c>).</summary>
    string? UserId { get; }

    /// <summary>Login name.</summary>
    string? Username { get; }

    /// <summary>E-mail.</summary>
    string? Email { get; }

    /// <summary>Display name.</summary>
    string? Name { get; }

    /// <summary>Tenant this request acts for, if any.</summary>
    TenantRef? Tenant { get; }

    /// <summary>All tenants the user belongs to.</summary>
    IReadOnlyList<TenantRef> Tenants { get; }

    /// <summary>Roles.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Authenticated with a second factor in this session (<c>acr = mfa</c>).</summary>
    bool HasMfa { get; }
}

/// <summary><see cref="ICurrentUser"/> and <see cref="ITenantContext"/> from the request's claims.</summary>
public sealed class ClaimsCurrentUser(IHttpContextAccessor accessor) : ICurrentUser, ITenantContext
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? new ClaimsPrincipal();

    /// <inheritdoc />
    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public string? UserId => User.FindFirst("sub")?.Value;

    /// <inheritdoc />
    public string? Username => User.FindFirst("preferred_username")?.Value;

    /// <inheritdoc />
    public string? Email => User.FindFirst("email")?.Value;

    /// <inheritdoc />
    public string? Name => User.FindFirst("name")?.Value;

    /// <inheritdoc />
    public TenantRef? Tenant =>
        User.FindFirst(NexoraClaims.TenantId)?.Value is { } id ? new TenantRef(id, User.FindFirst(NexoraClaims.TenantAlias)!.Value) : null;

    /// <inheritdoc />
    public IReadOnlyList<TenantRef> Tenants =>
        [.. User.FindAll(NexoraClaims.Membership).Select(c => c.Value.Split('|')).Select(p => new TenantRef(p[1], p[0]))];

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => [.. User.FindAll(NexoraClaims.Role).Select(c => c.Value)];

    /// <inheritdoc />
    public bool HasMfa => User.FindFirst("acr")?.Value == "mfa";

    /// <inheritdoc />
    public string? TenantId => Tenant?.Id;
}
