namespace Nexora.Modules.Platform.Contracts;

/// <summary>Public facts about the running platform.</summary>
/// <param name="Service">Service name.</param>
/// <param name="Version">Build version.</param>
/// <param name="Environment">Hosting environment (Development, Staging, Production …).</param>
public sealed record PlatformInfo(string Service, string Version, string Environment);
