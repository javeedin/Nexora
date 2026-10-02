using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexora.BuildingBlocks.Modules;
using Nexora.Modules.Platform.Contracts;

namespace Nexora.Modules.Platform;

/// <summary>The platform module (M0). Tenancy, identity, RBAC, settings and audit endpoints land here from P0-T06.</summary>
public sealed class PlatformModule : IModule
{
    /// <inheritdoc />
    public string Name => "platform";

    /// <inheritdoc />
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder group)
    {
        group.MapGet("/info", GetInfo)
            .WithName("GetPlatformInfo")
            .WithSummary("Service name, build version and environment.");
    }

    private static Ok<PlatformInfo> GetInfo(IHostEnvironment environment)
    {
        var version = typeof(PlatformModule).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
        return TypedResults.Ok(new PlatformInfo("nexora-api", version, environment.EnvironmentName));
    }
}
