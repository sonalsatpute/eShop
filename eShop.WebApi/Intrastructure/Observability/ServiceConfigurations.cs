using System.Diagnostics;
using System.Reflection;
using eShop.Observability;
using Microsoft.Extensions.Primitives;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.WebApi.Infrastructure.Observability;

public static class ServiceConfigurations
{
    private static void ConfigureTracing(AspNetCoreTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequest = (Action<Activity, HttpRequest>?)((activity, request) =>
        {
            var customHeaders = new[] { "TenantId", "SiteId", "X-Correlation-ID" };
            foreach (var header in customHeaders)
            {
                if (request.Headers.TryGetValue(header, out var headerValue))
                {
                    activity.SetTag($"http.request.header.{header}", headerValue.ToString());
                }
            }
            
            if (request == null || request.QueryString.HasValue == false) return;
                            
            activity.SetTag("url.query",  request.QueryString.Value);
            var queryParameters = request.Query;
                                    
            foreach (KeyValuePair<string, StringValues> param in queryParameters)
            {
                activity.SetTag($"url.query.{param.Key}", param.Value);
            }
        });
    }
    
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var collector_endpoint = new Uri(builder.Configuration.GetValue<string>("open_telemetry_collector_url")!);

        builder.Services.AddSingleton<IApplicationTypeChecker, ApplicationTypeChecker>();
        builder.Services.AddSingleton<IObservabilityOptions, ObservabilityOptions>();
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
                        .AddAspNetCoreInstrumentation(ConfigureTracing)
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.EnrichWithHttpRequestMessage = (activity, request) =>
                            {
                                if (request?.RequestUri?.Query.Length > 0)
                                {
                                    activity.SetTag("url.query", request.RequestUri.Query);
                                }
                            };
                            
                            options.FilterHttpRequestMessage = (request) =>
                            {
                                string absolutePath = request?.RequestUri?.AbsolutePath ?? string.Empty;

                                return !absolutePath.EndsWith(IGNORED_HEALTH_ENDPOINT, StringComparison.OrdinalIgnoreCase);
                            };
                        })
                        // .AddProcessor<ApiContextTraceProcessor>()
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

    private const string IGNORED_HEALTH_ENDPOINT = "/health";
}