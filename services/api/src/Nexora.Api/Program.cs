// Modular-monolith host. Skeleton only: module registration, OpenAPI, health, OTel etc. arrive with P0-T05.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "nexora-api" }));

app.Run();

/// <summary>Entry point marker, public so integration tests can host the API in-process.</summary>
public partial class Program;
