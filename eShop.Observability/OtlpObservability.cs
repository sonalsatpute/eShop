using Microsoft.Extensions.DependencyInjection;
using eShop.Observability.Configurations;
using OpenTelemetry;
using OpenTelemetry.Resources;

namespace eShop.Observability;

public interface IObservability
{
    IServiceCollection Configure(IServiceCollection services, IObservabilityConfigurator? configurator);
}

public class OtlpObservability : IObservability
{
    private readonly IObservabilityOptions _options;
    private readonly IResourceConfiguration _resource;
    private readonly ITracingConfiguration _tracing;
    private readonly IMetricsConfiguration _metrics;
    private readonly ILoggingConfiguration _logging;

    public OtlpObservability(
        IObservabilityOptions options,
        IResourceConfiguration resource,
        ITracingConfiguration tracing,
        IMetricsConfiguration metrics,
        ILoggingConfiguration logging
    )
    {
        _options = options;
        _resource = resource;
        _tracing = tracing;
        _metrics = metrics;
        _logging = logging;
    }

    public IServiceCollection Configure(IServiceCollection services,
        IObservabilityConfigurator? observabilityConfigurator)
    {
        services.AddSingleton<IApplicationMetric, ApplicationMetric>();
        
        if (!_options.IsObservabilityEnabled) return services;
        
        return _options.ForWebApp
            ? ConfigureWebApp(services, observabilityConfigurator) 
            : ConfigureConsoleApp(services, observabilityConfigurator);
    }
    
    private IServiceCollection ConfigureConsoleApp(IServiceCollection services,
        IObservabilityConfigurator? configurator)
    {
        ResourceBuilder resource = ResourceBuilder.CreateDefault();

        if (configurator?.ConfigureResources != null)
            configurator.ConfigureResources.Invoke(resource, _options);
        else
            _resource.Configure(resource);
        
        _tracing.Configure(resource, configurator?.ConfigureTracer);
        _metrics.Configure(resource, configurator?.ConfigureMeter);
        return _logging.Configure(services, resource, configurator?.ConfigureLogger);
    }

    private IServiceCollection ConfigureWebApp(IServiceCollection services,
        IObservabilityConfigurator? configurator)
    {
        OpenTelemetryBuilder builder = services.AddOpenTelemetry();
        builder.ConfigureResource(resource => _resource.Configure(resource));
        
        if (configurator?.ConfigureResources != null)
            builder.ConfigureResource(resource => configurator.ConfigureResources.Invoke(resource, _options));
        else
            builder.ConfigureResource(rb=> _resource.Configure(rb));

        _tracing.Configure(builder, configurator?.ConfigureTracer);
        _metrics.Configure(builder, configurator?.ConfigureMeter);
        _logging.Configure(builder, configurator?.ConfigureLogger);
        
        return services;
    }
}