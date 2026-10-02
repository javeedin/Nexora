using System.Diagnostics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexora.BuildingBlocks.Tenancy;

namespace Nexora.BuildingBlocks.Http;

/// <summary>Problem-details errors and rate limiting shared by every module.</summary>
public static class HttpConventions
{
    /// <summary>RFC 9457 problem details for every error, with the trace id so support can find the trace.</summary>
    public static IServiceCollection AddNexoraProblemDetails(this IServiceCollection services) =>
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
            var traceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
            context.ProblemDetails.Extensions["traceId"] = traceId;
        });

    /// <summary>
    /// Token bucket per tenant (or per client address before a tenant is known). Per-tenant / per-plan limits from
    /// entitlements arrive with P0-T07 / P0-T09. Config: <c>RateLimiting:PermitsPerMinute</c> (default 600).
    /// </summary>
    public static IServiceCollection AddNexoraRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
            {
                var tenant = http.RequestServices.GetRequiredService<ITenantContext>();
                var partition = tenant.TenantId ?? $"ip:{http.Connection.RemoteIpAddress}";
                var permits = http.RequestServices.GetRequiredService<IConfiguration>().GetValue("RateLimiting:PermitsPerMinute", 600);
                return RateLimitPartition.GetTokenBucketLimiter(partition, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = permits,
                    TokensPerPeriod = permits,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
            });
            options.OnRejected = async (context, cancellationToken) =>
            {
                var http = context.HttpContext;
                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    http.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                await http.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = http,
                    ProblemDetails = { Status = 429, Title = "Too many requests", Detail = "Rate limit exceeded; retry later." },
                });
            };
        });
}
