using OpenTelemetry.Resources;

namespace eShop.Observability;

internal interface IResourceConfiguration
{
    void Configure(ResourceBuilder resource);
}

internal class ResourceConfiguration : IResourceConfiguration
{
    private readonly IObservabilityOptions _options;

    public ResourceConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }

    public void Configure(ResourceBuilder resource)
    {
        resource.AddService(
                serviceName: _options.ServiceName,
                serviceVersion: _options.ServiceVersion,
                serviceInstanceId: Environment.MachineName
            );

        if (!_options.ForWebApp) 
            resource.Build();
    }
}