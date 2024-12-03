using eShop.Observability;
using eShop.Observability.Configurations;
using eShop.Observability.DaemonService;
using eShop.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SubmarineServiceSetup : IDaemonServiceSetup
{
    public IConfigurationBuilder BuildConfiguration(IConfigurationBuilder configBuilder) => configBuilder;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) => 
        services.AddObservabilityX(configuration, forWebApp: false);

    public void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
    }

    public void Configure(IHostBuilder builder)
    {
    }
}

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservabilityX(this IServiceCollection services, IConfiguration configuration, bool forWebApp)
    {
        IObservabilityOptions options = new ObservabilityOptions(configuration, forWebApp);
        IResourceConfiguration resourceConfiguration = new ResourceConfiguration(options);
        ITracingConfiguration tracingConfiguration = new TracingConfiguration(options);
        IMetricsConfiguration metricsConfiguration = new MetricsConfiguration(options);
        ILoggingConfiguration loggingConfiguration = new LoggingConfiguration(options);
        
        
        IObservability observability = new Observability(
            options,
            resourceConfiguration,
            tracingConfiguration,
            metricsConfiguration,
            loggingConfiguration
        );
        
        services.AddSingleton<IObservabilityOptions>(options);
        services.AddSingleton<IResourceConfiguration>(resourceConfiguration);
        services.AddSingleton<ITracingConfiguration>(tracingConfiguration);
        services.AddSingleton<IMetricsConfiguration>(metricsConfiguration);
        services.AddSingleton<ILoggingConfiguration>(loggingConfiguration);
        services.AddSingleton<IObservability>(observability);
        
        observability.Configure(services, null!);
            
        return services;
    }
}