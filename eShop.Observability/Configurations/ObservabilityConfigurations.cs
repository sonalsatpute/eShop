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
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services
            .AddSingleton<IApplicationTypeChecker>(provider => new ApplicationTypeChecker(provider))
            .AddSingleton<ICaptureHeader, CaptureHeader>()
            .AddSingleton<IObservabilityOptions, ObservabilityOptions>()
            .AddSingleton<IResourceConfiguration, ResourceConfiguration>()
            .AddSingleton<ITracingConfiguration, TracingConfiguration>()
            .AddSingleton<IMetricsConfiguration, MetricsConfiguration>()
            .AddSingleton<ILoggingConfiguration, LoggingConfiguration>()
            .AddSingleton<IObservability, Observability>();
        
        return services.ConfigureObservability();
    }

    private static IServiceCollection ConfigureObservability(this IServiceCollection services)
    {
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IObservability observability = serviceProvider.GetRequiredService<IObservability>();
        observability.Configure(services);
        
        return services;
    }
}