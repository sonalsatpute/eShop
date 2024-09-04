using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.WebApi.Infrastructure.Observability;

public static class ServiceConfigurations
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var collector_endpoint = new Uri(builder.Configuration.GetValue<string>("otlp_collector_endpoint")!);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(ApplicationDiagnostics.ServiceName)
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("service.version",
                            Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    });
            })
            .WithTracing(tracing =>
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter() // Add Console exporter for development
                        .AddOtlpExporter(options => options.Endpoint = collector_endpoint)
            )
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation()
                    // // Metrics provides by ASP.NET
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel") // Only Supported on .NET 8+
                    .AddMeter(ApplicationDiagnostics.Meter.Name)
                    .AddConsoleExporter() // Add Console exporter for development
                    .AddOtlpExporter(options => options.Endpoint = collector_endpoint)
            )
            .WithLogging(
                logging=>
                    logging
                        .AddConsoleExporter() // Add Console exporter for development
                        .AddOtlpExporter(options => options.Endpoint = collector_endpoint)
            );

        return builder;
    }
}