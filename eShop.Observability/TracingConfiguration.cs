using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.Observability;

internal interface ITracingConfiguration
{
    void Configure(IOpenTelemetryBuilder builder);
    void Configure(ResourceBuilder resource);
}

internal class TracingConfiguration : ITracingConfiguration
{
    private readonly IObservabilityOptions _options;
    private readonly ICaptureHeader _captureHeader;

    public TracingConfiguration(IObservabilityOptions options,
        ICaptureHeader captureHeader)
    {
        _options = options;
        _captureHeader = captureHeader;
    }
    
    public void Configure(IOpenTelemetryBuilder builder)
    {
        if (!_options.IsTracingEnabled) return;
        
        builder.WithTracing(tracing => ConfigureTracing(tracing));
    }

    public void Configure(ResourceBuilder resource)
    {
        if (!_options.IsTracingEnabled) return;
        
        var _tracing = Sdk.CreateTracerProviderBuilder();
        _tracing.SetResourceBuilder(resource);
        ConfigureTracing(_tracing, true);
    }

    
    private void ConfigureTracing(TracerProviderBuilder builder, bool isConsoleApp = false)
    {
        if (!isConsoleApp)
            builder
                .AddAspNetCoreInstrumentation(ConfigureAspNetCoreTraceInstrumentationOptions);

        builder
            // .AddProcessor<ApiContextTraceProcessor>()
            .AddHttpClientInstrumentation(ConfigureHttpClientTraceInstrumentationOptions)
            .AddAWSInstrumentation()
            .AddRedisInstrumentation()
            .AddSource(_options.ActivitySourceName)
            .AddConsoleExporter() // Add Console exporter for development
            .AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);
        
        if (isConsoleApp) builder.Build();
    }
    
    private void ConfigureHttpClientTraceInstrumentationOptions(HttpClientTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequestMessage = (activity, request) =>
        {
            if (request?.RequestUri?.Query.Length > 0)
            {
                activity.SetTag(ObservabilityConstants.URL_QUERY, request.RequestUri.Query);
            }
            
            _captureHeader.SetTags(activity, request?.Headers!, true);
        };
        
        options.EnrichWithHttpResponseMessage = (activity, response) =>
        {
            _captureHeader.SetTags(activity, response?.Headers!);
        };

        options.FilterHttpRequestMessage = (request) =>
        {
            string absolutePath = request?.RequestUri?.AbsolutePath ?? string.Empty;

            return !absolutePath.EndsWith(ObservabilityConstants.IGNORED_HEALTH_ENDPOINT,
                StringComparison.OrdinalIgnoreCase);
        };
    }

    private void ConfigureAspNetCoreTraceInstrumentationOptions(AspNetCoreTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequest = (Action<Activity, HttpRequest>)((activity, request) =>
        {
            if (request.QueryString.HasValue == false) return;
            _captureHeader.SetTags(activity, request.Headers, true);
                            
            activity.SetTag(ObservabilityConstants.URL_QUERY,  request.QueryString.Value);

            foreach (KeyValuePair<string, StringValues> param in request.Query)
            {
                activity.SetTag($"{ObservabilityConstants.URL_QUERY}.{param.Key}", param.Value);
            }
        });
        
        options.EnrichWithHttpResponse = (Action<Activity, HttpResponse>)((activity, response) =>
        {
            _captureHeader.SetTags(activity, response.Headers);
        });
    }
}