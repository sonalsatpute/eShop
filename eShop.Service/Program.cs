using eShop.Observability.Configurations;
using eShop.Observability.DaemonService;
using eShop.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    // public static async Task Main(string[] args)
    // {
    //     await CreateHostBuilder(args).Build().RunAsync();
    // }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<SampleHostedService>();
                services.AddObservability(hostContext.Configuration, forWebApp: false);
                services.AddHttpClient(); // Register HttpClient
            });
    
    
    public static async Task Main(string[] args)
    {
        await DaemonServiceFactory.StartAsync<SubmarineServiceSetup, SampleHostedService>(args);
    }
}