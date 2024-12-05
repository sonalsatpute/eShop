using Microsoft.Extensions.Configuration;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.Observability.Configurations;

public interface IObservabilityConfigurator
{
    Func<IConfiguration, IObservabilityOptions> ConfigureOptions { get; }
    Action<ResourceBuilder, IObservabilityOptions> ConfigureResources { get; }
    Action<TracerProviderBuilder, IObservabilityOptions> ConfigureTracer { get; }
    Action<MeterProviderBuilder> ConfigureMeter { get; }
    Action<LoggerProviderBuilder> ConfigureLogger { get; }
}

public class ObservabilityConfigurator : IObservabilityConfigurator
{
    public ObservabilityConfigurator(
        Func<IConfiguration, IObservabilityOptions> configureOptions = null!,
        Action<ResourceBuilder, IObservabilityOptions> configureResources = null!,
        Action<TracerProviderBuilder, IObservabilityOptions> configureTracer = null!,
        Action<MeterProviderBuilder> configureMeter = null!,
        Action<LoggerProviderBuilder> configureLogger = null!
    )
    {
        ConfigureOptions = configureOptions;
        ConfigureResources = configureResources;
        ConfigureTracer = configureTracer;
        ConfigureMeter = configureMeter;
        ConfigureLogger = configureLogger;
    }
    
    public Func<IConfiguration, IObservabilityOptions> ConfigureOptions { get; }
    public Action<ResourceBuilder, IObservabilityOptions> ConfigureResources { get; }
    public Action<TracerProviderBuilder, IObservabilityOptions> ConfigureTracer { get; }
    public Action<MeterProviderBuilder> ConfigureMeter { get; }
    public Action<LoggerProviderBuilder> ConfigureLogger { get; }
}