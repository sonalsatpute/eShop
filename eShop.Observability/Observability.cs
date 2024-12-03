using eShop.Observability.Configurations;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;

namespace eShop.Observability;

public interface IObservability
{
    IServiceCollection Configure(IServiceCollection services, IObservabilityConfigurator configurator);
}

public class Observability : IObservability
{
    private readonly IObservabilityOptions _options;
    private readonly IResourceConfiguration _resource;
    private readonly ITracingConfiguration _tracing;
    private readonly IMetricsConfiguration _metrics;
    private readonly ILoggingConfiguration _logging;

    public Observability(
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
        IObservabilityConfigurator observabilityConfigurator)
    {
        if (!_options.IsObservabilityEnabled) return services;
        
        return _options.ForWebApp
            ? ConfigureWebApp(services, observabilityConfigurator) 
            : ConfigureConsoleApp(services, observabilityConfigurator);
    }
    
    private IServiceCollection ConfigureConsoleApp(IServiceCollection services,
        IObservabilityConfigurator configurator)
    {
        ResourceBuilder resource = ResourceBuilder.CreateDefault();

        if (configurator?.ConfigureResources != null)
            configurator.ConfigureResources.Invoke(resource);
        else
            _resource.Configure(resource);
        
        _tracing.Configure(resource, configurator?.ConfigureTracerProvider);
        _metrics.Configure(resource, configurator?.ConfigureMeterProvider);
        return _logging.Configure(services, resource, configurator?.ConfigureLoggerProvider);
    }

    private IServiceCollection ConfigureWebApp(IServiceCollection services,
        IObservabilityConfigurator configurator)
    {
        OpenTelemetryBuilder builder = services.AddOpenTelemetry();
        builder.ConfigureResource(resource => _resource.Configure(resource));
        
        if (configurator?.ConfigureResources != null)
            builder.ConfigureResource(configurator.ConfigureResources);
        else
            builder.ConfigureResource(rb=> _resource.Configure(rb));

        _tracing.Configure(builder, configurator?.ConfigureTracerProvider);
        _metrics.Configure(builder, configurator?.ConfigureMeterProvider);
        _logging.Configure(builder, configurator?.ConfigureLoggerProvider);
        
        return services;
    }
}
