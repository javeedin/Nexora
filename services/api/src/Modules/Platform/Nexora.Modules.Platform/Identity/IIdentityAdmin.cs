namespace Nexora.Modules.Platform.Identity;

/// <summary>Administration calls to the identity provider (Keycloak, ADR 0010).</summary>
public interface IIdentityAdmin
{
    /// <summary>E-mails an invitation to join tenant <paramref name="tenantId"/> (its organisation).</summary>
    /// <exception cref="IdentityConflictException">The person is already a member.</exception>
    Task InviteToTenantAsync(string tenantId, string email, string? firstName, string? lastName, CancellationToken cancellationToken);
}

/// <summary>The identity provider refused because of existing state (e.g. already a member).</summary>
public sealed class IdentityConflictException(string message) : Exception(message);
