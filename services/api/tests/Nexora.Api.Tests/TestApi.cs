using System.Collections.Concurrent;
using System.Net.Http.Headers;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexora.BuildingBlocks.Http;
using Nexora.BuildingBlocks.Idempotency;
using Nexora.BuildingBlocks.Modules;
using Nexora.BuildingBlocks.Security;
using Nexora.Modules.Platform.Identity;

namespace Nexora.Api.Tests;

public sealed record CreateThing(string Name, int Quantity);

public sealed class CreateThingValidator : AbstractValidator<CreateThing>
{
    public CreateThingValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1000);
    }
}

/// <summary>Test-only module with write endpoints that exercise the cross-cutting pipeline.</summary>
public sealed class TestModule : IModule
{
    private int _executions;

    public int Executions => Volatile.Read(ref _executions);
    public TaskCompletionSource SlowGate { get; set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string Name => "test";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder group)
    {
        group.MapPost("/things", (CreateThing body) =>
            {
                var n = Interlocked.Increment(ref _executions);
                return TypedResults.Created($"/things/{n}", new { id = n, body.Name, body.Quantity });
            })
            .RequireAuthorization(NexoraPolicies.TenantMember)
            .WithValidation<CreateThing>()
            .WithIdempotency();

        group.MapPost("/fail", () =>
            {
                Interlocked.Increment(ref _executions);
                return TypedResults.Problem("boom", statusCode: 500);
            })
            .RequireAuthorization(NexoraPolicies.TenantMember)
            .WithIdempotency();

        group.MapPost("/slow", async () =>
            {
                Interlocked.Increment(ref _executions);
                await SlowGate.Task;
                return TypedResults.Ok();
            })
            .RequireAuthorization(NexoraPolicies.TenantMember)
            .WithIdempotency();
    }
}

/// <summary>API host with the test module plugged in; optional config overrides.</summary>
public sealed class TestApi : WebApplicationFactory<Program>
{
    public Dictionary<string, string?>? Settings { get; init; }

    public FakeIdentityAdmin Identity { get; } = new();

    /// <summary>Validates real Keycloak tokens instead of test-signed ones (Testcontainers test).</summary>
    public string? RealAuthority { get; init; }

    public TestModule Module { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var settings = new Dictionary<string, string?>(Settings ?? []);
        if (RealAuthority is not null)
        {
            settings["Auth:Authority"] = RealAuthority;
            settings["Auth:RequireHttpsMetadata"] = "false";
        }

        // Plain-HTTP metadata is allowed in Development only — the real-Keycloak test needs it.
        builder.UseEnvironment(RealAuthority is null ? "Testing" : "Development");
        builder.ConfigureAppConfiguration(c => c.AddInMemoryCollection(settings));

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IModule>(Module);
            services.AddScoped<IValidator<CreateThing>, CreateThingValidator>();
            services.AddSingleton<IIdentityAdmin>(Identity);
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwt =>
            {
                if (RealAuthority is not null)
                {
                    return;
                }

                jwt.Authority = null;
                jwt.TokenValidationParameters.ValidIssuer = TestTokens.Issuer;
                jwt.TokenValidationParameters.ValidAudience = TestTokens.Audience;
                jwt.TokenValidationParameters.IssuerSigningKey = TestTokens.Key;
            });
        });
    }

    /// <summary>Client sending a bearer token for <paramref name="user"/> (default: Acme admin) and optional tenant header.</summary>
    public HttpClient ClientFor(TestUser? user = null, string? tenant = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.For(user ?? TestUser.AcmeAdmin));
        if (tenant is not null)
        {
            client.DefaultRequestHeaders.Add("X-Nexora-Tenant", tenant);
        }

        return client;
    }

    public static HttpRequestMessage Post(string path, object? body, string? key = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(path, UriKind.Relative))
        {
            Content = body is null ? null : System.Net.Http.Json.JsonContent.Create(body),
        };
        if (key is not null)
        {
            request.Headers.Add(IdempotencyMiddleware.Header, key);
        }

        return request;
    }
}

/// <summary>Records invitations instead of calling Keycloak.</summary>
public sealed class FakeIdentityAdmin : IIdentityAdmin
{
    public ConcurrentQueue<(string TenantId, string Email)> Invitations { get; } = new();

    public HashSet<string> ExistingMembers { get; } = [];

    public Task InviteToTenantAsync(string tenantId, string email, string? firstName, string? lastName, CancellationToken cancellationToken)
    {
        if (ExistingMembers.Contains(email))
        {
            throw new IdentityConflictException($"{email} is already a member of this tenant.");
        }

        Invitations.Enqueue((tenantId, email));
        return Task.CompletedTask;
    }
}
