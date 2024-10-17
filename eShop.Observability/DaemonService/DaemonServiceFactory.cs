using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.Observability.DaemonService;

public interface IHostStartupFilter
{
    void Configure(IHost host);
}

public interface IDaemonServiceSetup
{
    IConfigurationBuilder BuildConfiguration(IConfigurationBuilder configBuilder);
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void ConfigureLogging(ILoggingBuilder loggingBuilder);
    void Configure(IHostBuilder builder);
}

public static class DaemonServiceFactory
    {
        public static Task  StartAsync<T,S>(string[] args, CancellationToken cancellationToken = default(CancellationToken))
            where T : IDaemonServiceSetup
            where S : class, IHostedService
        {
            return CreateHost<T, S>(args).RunAsync(cancellationToken);
        }
        
        public static Task StartAsync<S>(string[] args,  Func<IDaemonServiceSetup> startFactory , CancellationToken cancellationToken = default(CancellationToken))
            where S : class ,IHostedService
        {
            return CreateHost<S>(args,startFactory).RunAsync(cancellationToken);
        }
        public static IHost CreateHost<S>(string[] args, Func<IDaemonServiceSetup> startFactory )
            where S : class, IHostedService
        {
            IConfiguration configuration = null!;
            var startup = startFactory();
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    var aspnetEnv = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (!string.IsNullOrEmpty(aspnetEnv))
                    {
                        config.AddJsonFile($"appsettings.{aspnetEnv}.json", optional: true);
                    }
                    config.AddEnvironmentVariables();
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                    config = startup.BuildConfiguration(config);
                    configuration = config.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IHostedService, S>();
                    services.AddSingleton<IConfiguration>(c => configuration);
                    startup.ConfigureServices(services, configuration);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    startup.ConfigureLogging(logging);
                });


            startup.Configure(builder);

            IHost host = builder.Build();
            // Mozu.Core.Logging.LoggingService.LoggerFactory = host.Services.GetService<ILoggerFactory>();
            // Mozu.Core.Settings.MozuConfigurationManager.Settings = host.Services.GetService<ISettings>();
            // Mozu.Core.Localization.MozuStringLocalizer.InvariantCulture = host.Services.GetService<IMozuStringLocalizerFactory>().Create(null);
            //
            IEnumerable<IHostStartupFilter> hostStartupFilters = host.Services
                .GetService<IEnumerable<IStartupFilter>>()!
                .OfType<IHostStartupFilter>()
                .Reverse();
            foreach (var filter in hostStartupFilters)
            {
                filter.Configure(host);
            }

            return host;
        }

        public static IHost CreateHost<T, S>(string[] args)
            where T : IDaemonServiceSetup
            where S : class, IHostedService
        {
            return CreateHost<S>(args, ()=> (IDaemonServiceSetup)Activator.CreateInstance<T>());
        }
    }

