using Microsoft.Extensions.DependencyInjection;

namespace eShop.Observability;



internal class ObservabilityFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ObservabilityFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IObservability Create()
    {
        return _serviceProvider.GetRequiredService<IObservability>();
    }
}