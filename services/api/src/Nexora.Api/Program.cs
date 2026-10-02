using Microsoft.OpenApi;
using Nexora.BuildingBlocks.Http;
using Nexora.BuildingBlocks.Idempotency;
using Nexora.BuildingBlocks.Modules;
using Nexora.BuildingBlocks.Observability;
using Nexora.BuildingBlocks.Security;
using Nexora.Modules.Platform;
using Scalar.AspNetCore;

// Modular-monolith host (ADR 0003): cross-cutting HTTP conventions here, features inside modules.
var builder = WebApplication.CreateBuilder(args);

builder.AddNexoraTelemetry("nexora-api");
builder.Services.AddNexoraProblemDetails();
builder.Services.AddNexoraRateLimiting();
builder.Services.AddNexoraIdempotency();
builder.Services.AddNexoraHealth();
builder.Services.AddNexoraSecurity(); // OIDC (Keycloak, ADR 0010): user, tenant and roles from the token
builder.Services.AddOpenApi("v1", options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Nexora API";
    document.Info.Description = "Multi-tenant operations cloud for Oracle Fusion customers. Bearer token (OIDC) required "
        + "unless noted; write requests need an Idempotency-Key header; users in several tenants send X-Nexora-Tenant.";
    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
    {
        ["bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Access token from the Nexora identity provider (Keycloak realm `nexora`, ADR 0010).",
        },
    };
    document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("bearer", document)] = [] }];
    return Task.CompletedTask;
}));

builder.Services.AddModules(builder.Configuration, new PlatformModule());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseNexoraSecurity();   // authenticate; tenant + user on traces and logs
app.UseRateLimiter();      // partitions by tenant
app.UseAuthorization();    // authenticated by default; module policies
app.UseNexoraIdempotency(); // after authorization: unauthorized calls never claim keys

app.MapNexoraHealth();
app.MapOpenApi().AllowAnonymous();
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference("/docs").AllowAnonymous();
}

app.MapModules();

app.Run();

/// <summary>Entry point marker, public so integration tests can host the API in-process.</summary>
public partial class Program;
