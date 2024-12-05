using eShop.Observability;
using eShop.Observability.Configurations;
using eShop.Observability.DaemonService;
using eShop.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public class SubmarineServiceSetup : IDaemonServiceSetup
{
    public IConfigurationBuilder BuildConfiguration(IConfigurationBuilder configBuilder) => configBuilder;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) => 
        services.AddObservability(
            settings:configuration,
            // observabilityConfigurator: BuildObservabilityConfigurator(),
            forWebApp: false
        );

    public void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
    }

    public void Configure(IHostBuilder builder)
    {
    }
    
    public static IObservabilityConfigurator BuildObservabilityConfigurator()
    {
        IObservabilityOptions ConfigureOptions(IConfiguration settings) => 
            new CustomObservabilityOptions(settings: settings, forWebApp: false);
        
        void ConfigureTracerProvider(TracerProviderBuilder builder, IObservabilityOptions options)
        {
            builder.AddSource(options.ActivitySourceName);
            builder.AddAspNetCoreInstrumentation();
            builder.AddHttpClientInstrumentation();
            builder.AddConsoleExporter(); // Add Console exporter for development
            builder.AddOtlpExporter(op => op.Endpoint = options.CollectorEndpoint);

            builder.Build(); // Build the TracerProvider (only for console applications)
        }

        void ConfigureResources(ResourceBuilder builder, IObservabilityOptions options)
        {
            builder.AddService(
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion,
                serviceInstanceId: options.HostName
            );

            builder.Build(); // Build the resource (only for console applications)
        }

        IObservabilityConfigurator configurator = new ObservabilityConfigurator(
            configureOptions: ConfigureOptions
            ,configureResources: ConfigureResources
            , configureTracer: ConfigureTracerProvider
        );
        
        return configurator;
    }
}