using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        IResourceConfiguration resourceConfiguration = new ResourceConfiguration(options);
        ITracingConfiguration tracingConfiguration = new TracingConfiguration(options, new CaptureHeader());
        IMetricsConfiguration metricsConfiguration = new MetricsConfiguration(options);
        ILoggingConfiguration loggingConfiguration = new LoggingConfiguration(options);
        
        IObservability observability = new Observability(
            options,
            resourceConfiguration,
            tracingConfiguration,
            metricsConfiguration,
            loggingConfiguration
        );
        
        IObservabilityConfigurator observabilityConfigurator = new ObservabilityConfigurator(
            (_) => options,
            configureTracerProvider,
            configureResources,
            configureMeterProvider,
            configureLoggerProvider
        );  
        
        services.TryAddSingleton(options);
        // services.TryAddSingleton(resourceConfiguration);
        // services.TryAddSingleton(tracingConfiguration);
        // services.TryAddSingleton(metricsConfiguration);
        // services.TryAddSingleton(loggingConfiguration);
        
        
        return observability.Configure(services, observabilityConfigurator);
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