using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    /// <param name="isConsoleApp">Indicates whether the application is a console application.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, bool isConsoleApp = false)
    {
        IObservabilityOptions options = new ObservabilityOptions(configuration, isConsoleApp);

        IObservability observability = new Observability(
            options,
            new ResourceConfiguration(options),
            new TracingConfiguration(options, new CaptureHeader()),
            new MetricsConfiguration(options),
            new LoggingConfiguration(options)
        );
        
        services.TryAddSingleton(options);
        
        return observability.Configure(services);
    }
}