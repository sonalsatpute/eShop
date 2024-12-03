using eShop.Observability.DaemonService;
using eShop.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    // public static void Main(string[] args)
    // {
    //     CreateHostBuilder(args).Build().Run();
    // }
    //
    // public static IHostBuilder CreateHostBuilder(string[] args) =>
    //     Host.CreateDefaultBuilder(args)
    //         .ConfigureServices((hostContext, services) =>
    //         {
    //             services
    //                 .AddObservabilityX(hostContext.Configuration, false)
    //                 .AddHostedService<SubmarineService>();
    //         });
    
    
    public static void Main(string[] args)
    {
        DaemonServiceFactory.StartAsync<SubmarineServiceSetup, SubmarineService>(args);
    }
}