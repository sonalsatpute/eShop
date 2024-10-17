using eShop.Observability.Configurations.eShop.Observability.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
using OpenTelemetry.Resources;

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
    public static IServiceCollection AddObservability(this IServiceCollection services, bool isWebApp = true)
    {
        // services.AddSingleton<IStartupFilter, ObservabilityStartupFilter>();

        // services.TryAddSingleton<IApplicationTypeChecker>(provider => 
        //     new ApplicationTypeChecker(provider));
        
        services.TryAddSingleton<ICaptureHeader, CaptureHeader>();
        services.TryAddSingleton<IObservabilityOptions, ObservabilityOptions>();
        services.TryAddSingleton<IResourceConfiguration, ResourceConfiguration>();
        services.TryAddSingleton<ITracingConfiguration, TracingConfiguration>();
        services.TryAddSingleton<IMetricsConfiguration, MetricsConfiguration>();
        services.TryAddSingleton<ILoggingConfiguration, LoggingConfiguration>();
        services.TryAddSingleton<IObservability, Observability>();
        // services.TryAddSingleton<IObservabilityConfigurator>(
        //     new ObservabilityConfigurator(null!, null!)
        //     );

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