using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nexora.BuildingBlocks.Modules;

/// <summary>Registration and mapping of <see cref="IModule"/>s.</summary>
public static class ModuleExtensions
{
    /// <summary>Current public API version prefix. New versions get their own group next to this one.</summary>
    public const string V1 = "/api/v1";

    /// <summary>Registers each module's services, its FluentValidation validators and the module itself.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration, params IModule[] modules)
    {
        ArgumentNullException.ThrowIfNull(modules);
        foreach (var module in modules)
        {
            module.Register(services, configuration);
            services.AddValidatorsFromAssembly(module.GetType().Assembly, includeInternalTypes: true);
            services.AddSingleton(module);
        }

        return services;
    }

    /// <summary>Maps every registered module under <c>/api/v1/{name}</c>, tagged with the module name.</summary>
    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        var v1 = app.MapGroup(V1);
        foreach (var module in app.ServiceProvider.GetServices<IModule>())
        {
            module.MapEndpoints(v1.MapGroup($"/{module.Name}").WithTags(module.Name));
        }

        return app;
    }
}
