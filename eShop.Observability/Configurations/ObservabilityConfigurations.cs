using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.Observability.Configurations;

/// <summary>
/// Provides configuration methods for setting up OpenTelemetry instrumentation and diagnostics services.
/// </summary>
public static class ObservabilityConfigurations
{
    /// <summary>
    /// Adds OpenTelemetry instrumentation to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the instrumentation to.</param>
    /// <param name="configuration">The configuration to use for the instrumentation.</param>
    /// <param name="forWebApp">Determines if host application is WebApp or Console App</param>
    /// <param name="observabilityConfiguration"></param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddObservability(this IServiceCollection services,
        IConfiguration configuration,
        bool forWebApp = true,
        Action<ResourceBuilder> configureResources = null!,
        Action<TracerProviderBuilder> configureTracerProvider = null!,
        Action<MeterProviderBuilder> configureMeterProvider = null!,
        Action<LoggerProviderBuilder> configureLoggerProvider = null!
    )
    {
        IObservabilityOptions options = new ObservabilityOptions(configuration, forWebApp);

        if (!options.IsObservabilityEnabled) return services;
        services.TryAddSingleton(options);

        ResourceBuilder resource = ResourceBuilder.CreateDefault();
        Configure(resource, options, configureTracerProvider);

        return services;


        // ICaptureHeader captureHeader = new CaptureHeader();
        // TracingConfiguration tracingConfiguration = new TracingConfiguration(options, captureHeader);
        // IResourceConfiguration resourceConfiguration = new ResourceConfiguration(options);
        // IMetricsConfiguration metricsConfiguration = new MetricsConfiguration(options);
        // ILoggingConfiguration loggingConfiguration = new LoggingConfiguration(options);
        //
        // IObservability observability = new Observability(
        //     options,
        //     resourceConfiguration,
        //     tracingConfiguration,
        //     metricsConfiguration,
        //     loggingConfiguration
        // );
        //
        // services.TryAddSingleton(options);
        // services.TryAddSingleton(tracingConfiguration);
        // services.TryAddSingleton(captureHeader);
        // services.TryAddSingleton(resourceConfiguration);
        // services.TryAddSingleton(metricsConfiguration);
        // services.TryAddSingleton(loggingConfiguration);
        //
        //
        // IObservabilityConfigurator observabilityConfigurator = new ObservabilityConfigurator(
        //     (_) => options,
        //     configureTracerProvider,
        //     configureResources,
        //     configureMeterProvider,
        //     configureLoggerProvider
        // );  
        //
        // services.TryAddSingleton(observabilityConfigurator);
        //
        // return observability.Configure(services, observabilityConfigurator);

        // var openTelemetryBuilder = services.AddOpenTelemetry();
        // var resource = new ResourceConfiguration(options);
        //
        // if (configureResources != null)
        // {
        //     openTelemetryBuilder.ConfigureResource(configureResources);
        // }
        // else
        // {
        //     openTelemetryBuilder.ConfigureResource(rb=> resource.Configure(rb));
        // }
        //
        //
        // if (options.IsTracingEnabled)
        // {
        //     if (configureTracerProvider != null)
        //     {
        //         openTelemetryBuilder.WithTracing(configureTracerProvider);
        //     }
        //     else
        //     {
        //         var traceBuilder = new TracingConfiguration(options, new CaptureHeader());
        //         traceBuilder.Configure(openTelemetryBuilder, configureTracerProvider);
        //     }
        // }
        // if (options.IsLoggingEnabled)
        // {
        //     if (configureLoggerProvider != null)
        //     {
        //         openTelemetryBuilder.WithLogging(configureLoggerProvider);
        //     }
        //     else
        //     {
        //         var loggerBuilder = new LoggingConfiguration(options);
        //         loggerBuilder.Configure(openTelemetryBuilder, configureLoggerProvider);
        //     }
        // }
        // if (options.IsMetricsEnabled)
        // {
        //     if (configureMeterProvider != null)
        //     {
        //         openTelemetryBuilder.WithMetrics(configureMeterProvider);
        //     }
        //     else
        //     {
        //         var meterBuilder = new MetricsConfiguration(options);
        //         meterBuilder.Configure(openTelemetryBuilder, configureMeterProvider);
        //     }
        // }

        // return services;
    }



    static void Configure(ResourceBuilder resource, IObservabilityOptions _options, Action<TracerProviderBuilder>? configureTracerProvider)
    {
        if (!_options.IsTracingEnabled) return;

        TracerProviderBuilder tracing = Sdk.CreateTracerProviderBuilder();

        if (configureTracerProvider != null)
            configureTracerProvider.Invoke(tracing);
        else
        {
            tracing.SetResourceBuilder(resource);
            ConfigureTracing(tracing, _options);
        }
    }


    static void ConfigureTracing(TracerProviderBuilder builder, IObservabilityOptions _options)
    {
        // if (_options.ForWebApp)
        builder.AddAspNetCoreInstrumentation(ConfigureAspNetCoreTraceInstrumentationOptions);

        // builder.AddProcessor<ApiContextTraceProcessor>()
        builder.AddHttpClientInstrumentation(ConfigureHttpClientTraceInstrumentationOptions);
        builder.AddAWSInstrumentation();
        builder.AddRedisInstrumentation();
        builder.AddSource(_options.ActivitySourceName);
        builder.AddConsoleExporter(); // Add Console exporter for development
        builder.AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);

        if (!_options.ForWebApp)
            builder.Build();
    }

    static void ConfigureHttpClientTraceInstrumentationOptions(HttpClientTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequestMessage = (activity, request) =>
        {
            if (request?.RequestUri?.Query.Length > 0)
            {
                activity.SetTag(ObservabilityConstants.URL_QUERY, request.RequestUri.Query);
            }

            // _captureHeader.SetTags(activity, request?.Headers!, true);
        };

        options.EnrichWithHttpResponseMessage = (activity, response) =>
        {
            // _captureHeader.SetTags(activity, response?.Headers!);
        };

        options.FilterHttpRequestMessage = (request) =>
        {
            string absolutePath = request?.RequestUri?.AbsolutePath ?? string.Empty;

            return !absolutePath.EndsWith(ObservabilityConstants.IGNORED_HEALTH_ENDPOINT,
                StringComparison.OrdinalIgnoreCase);
        };
    }

    static void ConfigureAspNetCoreTraceInstrumentationOptions(AspNetCoreTraceInstrumentationOptions options)
    {
        options.EnrichWithHttpRequest = (Action<Activity, HttpRequest>)((activity, request) =>
        {
            if (request.QueryString.HasValue == false) return;
            // _captureHeader.SetTags(activity, request.Headers, true);

            activity.SetTag(ObservabilityConstants.URL_QUERY, request.QueryString.Value);

            foreach (KeyValuePair<string, StringValues> param in request.Query)
            {
                activity.SetTag($"{ObservabilityConstants.URL_QUERY}.{param.Key}", param.Value);
            }
        });

        options.EnrichWithHttpResponse = (Action<Activity, HttpResponse>)((activity, response) =>
        {
            // _captureHeader.SetTags(activity, response.Headers);
        });
    }


}


public interface IObservabilityConfigurator
{
    Func<IConfiguration, IObservabilityOptions> ConfigureOptions { get; }
    Action<TracerProviderBuilder> ConfigureTracerProvider { get; }
    Action<ResourceBuilder> ConfigureResources { get; }
    Action<MeterProviderBuilder> ConfigureMeterProvider { get; }
    Action<LoggerProviderBuilder> ConfigureLoggerProvider { get; }
    IServiceCollection Configure(IServiceCollection services);
}

public class ObservabilityConfigurator : IObservabilityConfigurator
{
    public ObservabilityConfigurator()
    {
        ConfigureOptions = null!;
        ConfigureTracerProvider = null!;
        ConfigureResources = null!;
        ConfigureMeterProvider = null!;
        ConfigureLoggerProvider = null!;
    }
    
    public ObservabilityConfigurator(
        Func<IConfiguration, IObservabilityOptions> configureOptions = null!,
        Action<TracerProviderBuilder> configureTracerProvider = null!,
        Action<ResourceBuilder> configureResources = null!,
        Action<MeterProviderBuilder> configureMeterProvider = null!,
        Action<LoggerProviderBuilder> configureLoggerProvider = null!
        )
    {
        ConfigureOptions = configureOptions;
        ConfigureTracerProvider = configureTracerProvider;
        ConfigureResources = configureResources;
        ConfigureMeterProvider = configureMeterProvider;
        ConfigureLoggerProvider = configureLoggerProvider;
    }
    
    public Func<IConfiguration, IObservabilityOptions> ConfigureOptions { get; }
    public Action<TracerProviderBuilder> ConfigureTracerProvider { get; }
    public Action<ResourceBuilder> ConfigureResources { get; }
    public Action<MeterProviderBuilder> ConfigureMeterProvider { get; }
    public Action<LoggerProviderBuilder> ConfigureLoggerProvider { get; }
    public IServiceCollection Configure(IServiceCollection services)
    {
        return services;    
    }
}