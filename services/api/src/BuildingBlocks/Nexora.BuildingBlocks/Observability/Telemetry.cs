using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nexora.BuildingBlocks.Observability;

/// <summary>OpenTelemetry traces, metrics and logs (rule 6, NFR observability).</summary>
public static class Telemetry
{
    /// <summary>
    /// Instruments ASP.NET Core, HttpClient and the runtime. Exports over OTLP when
    /// <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is set (standard OTel variables apply: headers, protocol, sampler …).
    /// <c>tenant_id</c> is added to spans with the tenant context in P0-T07.
    /// </summary>
    public static IHostApplicationBuilder AddNexoraTelemetry(this IHostApplicationBuilder builder, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var otel = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: typeof(Telemetry).Assembly.GetName().Version?.ToString())
                .AddAttributes([new("deployment.environment.name", builder.Environment.EnvironmentName)]))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithLogging();

        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            otel.UseOtlpExporter();
        }

        return builder;
    }
}
