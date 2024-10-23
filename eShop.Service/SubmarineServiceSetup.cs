using eShop.Observability.Configurations;
using eShop.Observability.DaemonService;
using eShop.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class SubmarineServiceSetup : IDaemonServiceSetup
{
    public IConfigurationBuilder BuildConfiguration(IConfigurationBuilder configBuilder) => configBuilder;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) => 
        services.AddObservability(configuration,forWebApp: false);

    public void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
    }

    public void Configure(IHostBuilder builder)
    {
    }
}