using System;
using eShop.Observability.Constants;
using eShop.Observability.Tracing;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.Observability.Configurations;

public interface ITracingConfiguration
{
    void Configure(IOpenTelemetryBuilder builder, Action<TracerProviderBuilder, IObservabilityOptions>? configureTracer);
    void Configure(ResourceBuilder resource, Action<TracerProviderBuilder, IObservabilityOptions>? configureTracer);
}

public class DefaultTracingConfiguration : ITracingConfiguration
{
    private readonly IObservabilityOptions _options;

    public DefaultTracingConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }
    
    public void Configure(IOpenTelemetryBuilder builder, Action<TracerProviderBuilder, IObservabilityOptions>? configureTracer)
    {
        if (!_options.IsTracingEnabled) return;
        
        if (configureTracer != null)
            builder.WithTracing(provider => configureTracer.Invoke(provider, _options));
        else
            builder.WithTracing(ConfigureTracing);
    }

    public void Configure(ResourceBuilder resource, Action<TracerProviderBuilder, IObservabilityOptions>? configureTracer)
    {
        if (!_options.IsTracingEnabled) return;

        TracerProviderBuilder tracing = Sdk.CreateTracerProviderBuilder();
        tracing.SetResourceBuilder(resource);
        
        if (configureTracer != null)
            configureTracer.Invoke(tracing, _options);
        else
            ConfigureTracing(tracing);
    }

    private void ConfigureTracing(TracerProviderBuilder builder)
    {
        if (_options.ForWebApp)
            builder.AddAspNetCoreInstrumentation(ConfigureAspNetCoreInstrumentation);

        builder
            .AddHttpClientInstrumentation(ConfigureHttpClientInstrumentation)
            .AddAWSInstrumentation()
            .AddRedisInstrumentation()
            .AddSource(_options.ActivitySourceName)
            .AddConsoleExporter() // Add Console exporter for development
            .AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);
        
        if (_options.ForConsoleApp) builder.Build();
    }
    
    private void ConfigureHttpClientInstrumentation(HttpClientTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequestMessage = HttpRequestResponse.EnrichWithHttpRequestMessage;
        options.EnrichWithHttpResponseMessage = HttpRequestResponse.EnrichWithHttpResponseMessage;

        options.FilterHttpRequestMessage = (request) =>
        {
            string absolutePath = request?.RequestUri?.AbsolutePath ?? string.Empty;

            return !absolutePath.EndsWith(ObservabilityConstants.IGNORED_HEALTH_ENDPOINT, StringComparison.OrdinalIgnoreCase);
        };
    }

    private void ConfigureAspNetCoreInstrumentation(AspNetCoreTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequest = HttpRequestResponse.EnrichWithHttpRequest;
        options.EnrichWithHttpResponse = HttpRequestResponse.EnrichWithHttpResponse;
    }
}