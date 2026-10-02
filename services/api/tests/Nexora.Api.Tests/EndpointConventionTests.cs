using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nexora.BuildingBlocks.Idempotency;
using Nexora.BuildingBlocks.Modules;

namespace Nexora.Api.Tests;

/// <summary>Conventions every real endpoint must follow — fails the build for new endpoints that skip them.</summary>
public sealed class EndpointConventionTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string[] WriteMethods = ["POST", "PUT", "PATCH", "DELETE"];

    private IEnumerable<RouteEndpoint> ApiEndpoints() =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText?.StartsWith(ModuleExtensions.V1, StringComparison.Ordinal) == true);

    [Fact]
    public void Every_write_endpoint_requires_an_idempotency_key_or_says_why_not()
    {
        using var client = factory.CreateClient(); // builds the host
        var offenders = ApiEndpoints()
            .Where(e => e.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods.Any(WriteMethods.Contains) == true)
            .Where(e => e.Metadata.GetMetadata<IdempotentAttribute>() is null && e.Metadata.GetMetadata<NotIdempotentMetadata>() is null)
            .Select(e => e.RoutePattern.RawText)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void Every_write_endpoint_names_its_authorization_policy()
    {
        // Writes must say who may perform them (tenant-member, tenant-admin, platform-admin …) instead of relying on
        // the "any signed-in user" fallback — a user without a tenant must never reach tenant data by default.
        using var client = factory.CreateClient();
        var offenders = ApiEndpoints()
            .Where(e => e.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods.Any(WriteMethods.Contains) == true)
            .Where(e => !e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(a => !string.IsNullOrEmpty(a.Policy)))
            .Select(e => e.RoutePattern.RawText)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void Every_api_endpoint_lives_under_api_v1_module_prefix()
    {
        using var client = factory.CreateClient();
        var modules = factory.Services.GetServices<IModule>().Select(m => $"{ModuleExtensions.V1}/{m.Name}/").ToList();
        Assert.NotEmpty(ApiEndpoints());
        Assert.All(ApiEndpoints(), e => Assert.Contains(modules, prefix => e.RoutePattern.RawText!.StartsWith(prefix, StringComparison.Ordinal)));
    }
}
