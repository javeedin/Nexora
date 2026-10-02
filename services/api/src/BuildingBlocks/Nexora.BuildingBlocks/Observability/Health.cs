using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nexora.BuildingBlocks.Caching;
using StackExchange.Redis;

namespace Nexora.BuildingBlocks.Observability;

/// <summary>Liveness (process up) and readiness (dependencies reachable) for Kubernetes probes.</summary>
public static class Health
{
    /// <summary>Tag for checks that must pass before the instance receives traffic.</summary>
    public const string Ready = "ready";

    /// <summary>Registers health checks. Readiness includes Redis when it is configured.</summary>
    public static IServiceCollection AddNexoraHealth(this IServiceCollection services)
    {
        services.TryAddSingleton<RedisConnection>();
        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", tags: [Ready]);
        return services;
    }

    /// <summary>Maps <c>/health/live</c> and <c>/health/ready</c> (not rate limited, not in the public API document).</summary>
    public static IEndpointRouteBuilder MapNexoraHealth(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false, ResponseWriter = WriteJson })
            .DisableRateLimiting().ExcludeFromDescription().AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains(Ready), ResponseWriter = WriteJson })
            .DisableRateLimiting().ExcludeFromDescription().AllowAnonymous();
        return app;
    }

    private static Task WriteJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        return JsonSerializer.SerializeAsync(context.Response.Body, new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()),
        });
    }

    private sealed class RedisHealthCheck(RedisConnection connection) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (connection.Multiplexer is not { } redis)
                {
                    return HealthCheckResult.Healthy("not configured");
                }

                var latency = await redis.GetDatabase().PingAsync();
                return HealthCheckResult.Healthy($"ping {latency.TotalMilliseconds:F0} ms");
            }
            catch (RedisException ex)
            {
                return HealthCheckResult.Unhealthy("redis unreachable", ex);
            }
        }
    }
}
