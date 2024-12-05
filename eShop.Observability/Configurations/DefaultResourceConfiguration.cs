using OpenTelemetry.Resources;

namespace eShop.Observability.Configurations;

public interface IResourceConfiguration
{
    void Configure(ResourceBuilder resource, bool isConsoleApp = false);
}

public class DefaultResourceConfiguration : IResourceConfiguration
{
    private readonly IObservabilityOptions _options;

    public DefaultResourceConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }

    public void Configure(ResourceBuilder resource, bool isConsoleApp = false)
    {
        resource.AddService(
                serviceName: _options.ServiceName,
                serviceVersion: _options.ServiceVersion,
                serviceInstanceId: _options.HostName
            );

        if (_options.ForConsoleApp) resource.Build();
    }
}