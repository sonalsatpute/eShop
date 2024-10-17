using eShop.Observability.DaemonService;
using eShop.Service.Services;

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
    //                 .AddObservability()
    //                 .AddHostedService<SubmarineService>();
    //         });
    
    
    public static void Main(string[] args)
    {
        DaemonServiceFactory.StartAsync<SubmarineServiceSetup, SubmarineService>(args);
    }
}