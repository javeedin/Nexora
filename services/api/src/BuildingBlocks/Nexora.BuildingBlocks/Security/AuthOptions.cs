namespace Nexora.BuildingBlocks.Security;

/// <summary>Configuration section <c>Auth</c>.</summary>
public sealed class AuthOptions
{
    /// <summary>OIDC issuer, e.g. <c>https://id.example.com/realms/nexora</c>.</summary>
    public string? Authority { get; set; }

    /// <summary>Expected <c>aud</c>.</summary>
    public string Audience { get; set; } = "nexora-api";

    /// <summary>HTTPS for the metadata endpoint; only Development may turn it off.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>Admin policies require <c>acr = mfa</c> (password + OTP). On by default.</summary>
    public bool RequireMfaForAdmins { get; set; } = true;
}
