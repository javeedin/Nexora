using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexora.BuildingBlocks.Caching;
using Nexora.BuildingBlocks.Tenancy;

namespace Nexora.BuildingBlocks.Idempotency;

/// <summary>Endpoint metadata: the endpoint requires an <c>Idempotency-Key</c> header.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class IdempotentAttribute : Attribute;

/// <summary>Endpoint metadata: a write endpoint that deliberately has no idempotency key, with the reason.</summary>
/// <param name="Reason">Why (reviewed in code review; listed by the architecture test).</param>
public sealed record NotIdempotentMetadata(string Reason);

/// <summary>
/// Idempotency keys on write APIs (RD 03 §3.4). Same key + same request → stored response replayed
/// (<c>Idempotent-Replayed: true</c>); same key + different request → 422; still executing → 409; 5xx responses are
/// not stored so the client can retry. Keys are scoped per tenant, method and route.
/// </summary>
public sealed partial class IdempotencyMiddleware(RequestDelegate next)
{
    /// <summary>Request header name.</summary>
    public const string Header = "Idempotency-Key";

    private static readonly TimeSpan LockFor = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan KeepFor = TimeSpan.FromHours(24);

    /// <summary>Handles one request.</summary>
    public async Task InvokeAsync(HttpContext context, IIdempotencyStore store, ITenantContext tenant)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.GetEndpoint()?.Metadata.GetMetadata<IdempotentAttribute>() is null)
        {
            await next(context);
            return;
        }

        var key = context.Request.Headers[Header].ToString();
        if (!KeyFormat().IsMatch(key))
        {
            await Problem(context, StatusCodes.Status400BadRequest, $"{Header} header required",
                "Write requests need a unique key of 8–128 characters [A-Za-z0-9_.:-], e.g. a UUID.");
            return;
        }

        var ct = context.RequestAborted;
        var fingerprint = await Fingerprint(context.Request, ct);
        var storeKey = $"nexora:idem:{tenant.Scope}:{context.Request.Method}:{context.Request.Path}:{key}";
        var claim = await store.ClaimAsync(storeKey, fingerprint, LockFor, ct);
        switch (claim.Outcome)
        {
            case ClaimOutcome.Mismatch:
                await Problem(context, StatusCodes.Status422UnprocessableEntity, "Idempotency key reused",
                    "This key was already used for a different request.");
                return;
            case ClaimOutcome.InProgress:
                await Problem(context, StatusCodes.Status409Conflict, "Request in progress",
                    "A request with this idempotency key is still being processed.");
                return;
            case ClaimOutcome.Completed:
                var stored = claim.Response!;
                context.Response.StatusCode = stored.StatusCode;
                context.Response.ContentType = stored.ContentType;
                context.Response.Headers["Idempotent-Replayed"] = "true";
                await context.Response.Body.WriteAsync(stored.Body, ct);
                return;
        }

        var original = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        try
        {
            await next(context);
        }
        catch
        {
            await store.ReleaseAsync(storeKey, CancellationToken.None);
            throw;
        }
        finally
        {
            context.Response.Body = original;
        }

        var response = new StoredResponse(context.Response.StatusCode, context.Response.ContentType, buffer.ToArray());
        if (response.StatusCode >= 500)
        {
            await store.ReleaseAsync(storeKey, CancellationToken.None);
        }
        else
        {
            await store.CompleteAsync(storeKey, fingerprint, response, KeepFor, CancellationToken.None);
        }

        await original.WriteAsync(response.Body, ct);
    }

    private static async Task<string> Fingerprint(HttpRequest request, CancellationToken ct)
    {
        request.EnableBuffering();
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha.AppendData(System.Text.Encoding.UTF8.GetBytes($"{request.Method} {request.Path}{request.QueryString}\n"));
        var chunk = new byte[8192];
        int read;
        while ((read = await request.Body.ReadAsync(chunk, ct)) > 0)
        {
            sha.AppendData(chunk, 0, read);
        }

        request.Body.Position = 0;
        return Convert.ToHexString(sha.GetHashAndReset());
    }

    private static Task Problem(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        return context.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = { Status = status, Title = title, Detail = detail },
        }).AsTask();
    }

    [GeneratedRegex("^[A-Za-z0-9_.:-]{8,128}$")]
    private static partial Regex KeyFormat();
}

/// <summary>Registration and endpoint conventions for idempotency.</summary>
public static class IdempotencyExtensions
{
    /// <summary>Uses Redis (<c>ConnectionStrings:Redis</c>) when configured, else an in-memory store.</summary>
    public static IServiceCollection AddNexoraIdempotency(this IServiceCollection services)
    {
        services.TryAddSingleton<RedisConnection>();
        services.AddSingleton<IIdempotencyStore>(sp => sp.GetRequiredService<RedisConnection>().Multiplexer is { } redis
            ? new RedisIdempotencyStore(redis)
            : new InMemoryIdempotencyStore(sp.GetService<TimeProvider>()));
        return services;
    }

    /// <summary>Requires an <c>Idempotency-Key</c> header on this endpoint.</summary>
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new IdempotentAttribute());

    /// <summary>Marks a write endpoint as deliberately not idempotent-keyed (reason is mandatory).</summary>
    public static TBuilder WithoutIdempotency<TBuilder>(this TBuilder builder, string reason)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return builder.WithMetadata(new NotIdempotentMetadata(reason));
    }

    /// <summary>Adds the middleware (after routing, so endpoint metadata is known).</summary>
    public static IApplicationBuilder UseNexoraIdempotency(this IApplicationBuilder app) =>
        app.UseMiddleware<IdempotencyMiddleware>();
}
