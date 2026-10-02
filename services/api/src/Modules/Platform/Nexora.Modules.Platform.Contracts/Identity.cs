namespace Nexora.Modules.Platform.Contracts;

/// <summary>A tenant reference.</summary>
/// <param name="Id">Tenant id (UUID).</param>
/// <param name="Alias">Tenant alias, e.g. <c>acme</c>.</param>
public sealed record TenantSummary(string Id, string Alias);

/// <summary>The signed-in user as the API sees them.</summary>
/// <param name="UserId">Stable user id.</param>
/// <param name="Username">Login name.</param>
/// <param name="Email">E-mail.</param>
/// <param name="Name">Display name.</param>
/// <param name="Tenant">Tenant this request acts for (null if the user must choose one).</param>
/// <param name="Tenants">All tenants of the user.</param>
/// <param name="Roles">Roles.</param>
/// <param name="Mfa">Signed in with a second factor.</param>
public sealed record MeResponse(
    string UserId, string? Username, string? Email, string? Name,
    TenantSummary? Tenant, IReadOnlyList<TenantSummary> Tenants, IReadOnlyList<string> Roles, bool Mfa);

/// <summary>Invite someone to the current tenant (PC-04).</summary>
/// <param name="Email">E-mail the invitation goes to.</param>
/// <param name="FirstName">Optional first name.</param>
/// <param name="LastName">Optional last name.</param>
public sealed record InviteUserRequest(string Email, string? FirstName = null, string? LastName = null);

/// <summary>Result of an invitation.</summary>
/// <param name="Email">Invited e-mail.</param>
/// <param name="TenantId">Tenant joined on acceptance.</param>
/// <param name="Status"><c>sent</c>.</param>
public sealed record InvitationResponse(string Email, string TenantId, string Status);
