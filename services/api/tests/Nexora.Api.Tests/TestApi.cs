using FluentValidation;
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
            .WithValidation<CreateThing>()
            .WithIdempotency();

        group.MapPost("/fail", () =>
            {
                Interlocked.Increment(ref _executions);
                return TypedResults.Problem("boom", statusCode: 500);
            })
            .WithIdempotency();

        group.MapPost("/slow", async () =>
            {
                Interlocked.Increment(ref _executions);
                await SlowGate.Task;
                return TypedResults.Ok();
            })
            .WithIdempotency();
    }
}

/// <summary>API host with the test module plugged in; optional config overrides.</summary>
public sealed class TestApi : WebApplicationFactory<Program>
{
    public Dictionary<string, string?>? Settings { get; init; }

    public TestModule Module { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        if (Settings is not null)
        {
            builder.ConfigureAppConfiguration(c => c.AddInMemoryCollection(Settings));
        }

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IModule>(Module);
            services.AddScoped<IValidator<CreateThing>, CreateThingValidator>();
        });
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
