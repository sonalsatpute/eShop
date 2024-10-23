using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
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
        
        if ( !options.IsObservabilityEnabled) return services;

        services.TryAddSingleton(options);

        IObservability observability = new Observability(
            options,
            new ResourceConfiguration(options),
            new TracingConfiguration(options, new CaptureHeader()),
            new MetricsConfiguration(options),
            new LoggingConfiguration(options)
        );
        
        IObservabilityConfigurator observabilityConfigurator = new ObservabilityConfigurator(
            (_) => options,
            configureTracerProvider,
            configureResources,
            configureMeterProvider,
            configureLoggerProvider
        );  
        
        return observability.Configure(services, observabilityConfigurator);
        
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