using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;

namespace eShop.Observability;

public interface IObservability
{
    IServiceCollection Configure(IServiceCollection services);
}

internal class Observability : IObservability
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

    public IServiceCollection Configure(IServiceCollection services)
    {
        if (!_options.IsObservabilityEnabled) return services;
        
        return IsWebApplication(services) 
            ? ConfigureWebApp(services) 
            : ConfigureConsoleApp(services);
    }

    public bool IsWebApplication(IServiceCollection services)
    {
        IApplicationTypeChecker typeCheckerChecker = GetServiceApplicationTypeChecker(services);
        return typeCheckerChecker.IsWebApp();
    }
    
    private static IApplicationTypeChecker GetServiceApplicationTypeChecker(IServiceCollection services)
    {
        // 🔥I know, I know, this is an anti-pattern!
        // ❓but it's the only way to get the service instance in static scope? 
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IApplicationTypeChecker typeChecker = serviceProvider.GetService<IApplicationTypeChecker>()!;
        return typeChecker;
    }
    
    private IServiceCollection ConfigureConsoleApp(IServiceCollection services)
    {
        ResourceBuilder resource = ResourceBuilder.CreateDefault();
        _resource.Configure(resource, true);
        
        _tracing.Configure(resource);
        _metrics.Configure(resource);
        return _logging.Configure(services, resource);
    }

    private IServiceCollection ConfigureWebApp(IServiceCollection services)
    {
        OpenTelemetryBuilder builder = services.AddOpenTelemetry();
        builder.ConfigureResource(resource => _resource.Configure(resource));

        _tracing.Configure(builder);
        _metrics.Configure(builder);
        _logging.Configure(builder);
        
        return services;
    }
}