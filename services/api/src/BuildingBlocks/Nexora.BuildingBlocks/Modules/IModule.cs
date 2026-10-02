using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nexora.BuildingBlocks.Modules;

/// <summary>
/// A module of the modular monolith (ADR 0003): own folder, own schema, public contracts only.
/// The host registers modules explicitly; each maps its endpoints under its own versioned prefix.
/// </summary>
public interface IModule
{
    /// <summary>URL segment and OpenAPI tag, e.g. <c>platform</c> → <c>/api/v1/platform</c>.</summary>
    string Name { get; }

    /// <summary>Registers the module's services. Called once at start-up.</summary>
    void Register(IServiceCollection services, IConfiguration configuration);

    /// <summary>Maps the module's endpoints onto <paramref name="group"/> (already <c>/api/v1/{Name}</c>).</summary>
    void MapEndpoints(IEndpointRouteBuilder group);
}
