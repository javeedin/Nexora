using Nexora.BuildingBlocks.Http;
using Nexora.BuildingBlocks.Idempotency;
using Nexora.BuildingBlocks.Modules;
using Nexora.BuildingBlocks.Observability;
using Nexora.BuildingBlocks.Tenancy;
using Nexora.Modules.Platform;
using Scalar.AspNetCore;

// Modular-monolith host (ADR 0003): cross-cutting HTTP conventions here, features inside modules.
var builder = WebApplication.CreateBuilder(args);

builder.AddNexoraTelemetry("nexora-api");
builder.Services.AddNexoraProblemDetails();
builder.Services.AddNexoraRateLimiting();
builder.Services.AddNexoraIdempotency();
builder.Services.AddNexoraHealth();
builder.Services.AddScoped<ITenantContext, NoTenantContext>(); // P0-T07: resolved from the token
builder.Services.AddOpenApi("v1", options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Nexora API";
    document.Info.Description = "Multi-tenant operations cloud for Oracle Fusion customers. Write requests need an Idempotency-Key header.";
    return Task.CompletedTask;
}));

builder.Services.AddModules(builder.Configuration, new PlatformModule());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();
app.UseNexoraIdempotency();

app.MapNexoraHealth();
app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference("/docs");
}

app.MapModules();

app.Run();

/// <summary>Entry point marker, public so integration tests can host the API in-process.</summary>
public partial class Program;
