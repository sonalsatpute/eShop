using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eShop.Observability.Configurations;

public static class ServiceConfigurations
{
    /// <summary>
    /// Adds OpenTelemetry instrumentation to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the instrumentation to</param>
    /// <param name="settings">Configuration/Settings</param>
    /// <param name="observabilityConfigurator">Override's default Open Telemetry Configuration</param>
    /// <param name="forWebApp">Indicate the host app is Web App or Console App</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddObservability(this IServiceCollection services,
        IConfiguration settings,  
        IObservabilityConfigurator? observabilityConfigurator= null,
        bool forWebApp = true
    )
    {
        IObservabilityOptions options = observabilityConfigurator?.ConfigureOptions?.Invoke(settings) 
                                        ?? new DefaultObservabilityOptions(settings, new ServiceInfoProvider(), forWebApp);
        
        if ( !options.IsObservabilityEnabled) return services;

        IResourceConfiguration resourceConfiguration = new DefaultResourceConfiguration(options);
        ITracingConfiguration tracingConfiguration = new DefaultTracingConfiguration(options);
        IMetricsConfiguration metricsConfiguration = new DefaultMetricsConfiguration(options);
        ILoggingConfiguration loggingConfiguration = new DefaultLoggingConfiguration(options);
        
        IObservability observability = new OtlpObservability(
            options,
            resourceConfiguration,
            tracingConfiguration,
            metricsConfiguration,
            loggingConfiguration
        );

        services.TryAddSingleton(options);
        return observability.Configure(services, observabilityConfigurator);
    }
}