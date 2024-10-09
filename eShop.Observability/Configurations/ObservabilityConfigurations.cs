using Microsoft.Extensions.DependencyInjection;

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
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services
            .AddSingleton<IApplicationTypeChecker, ApplicationTypeChecker>()
            .AddSingleton<ICaptureHeader, CaptureHeader>()
            .AddSingleton<IObservabilityOptions, ObservabilityOptions>()
            .AddSingleton<IResourceConfiguration, ResourceConfiguration>()
            .AddSingleton<ITracingConfiguration, TracingConfiguration>()
            .AddSingleton<IMetricsConfiguration, MetricsConfiguration>()
            .AddSingleton<ILoggingConfiguration, LoggingConfiguration>()
            .AddSingleton<IObservability, Observability>();

        
        IObservability observability = GetServiceObservability(services);
        
        return observability.Configure(services);
    }
    
    private static IObservability GetServiceObservability(IServiceCollection services)
    {
        // 🔥I know, I know, this is an anti-pattern!
        // ❓but it's the only way to get the service instance in static scope? 
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IObservability observabilityOptions = serviceProvider.GetService<IObservability>()!;
        return observabilityOptions;
    }
}