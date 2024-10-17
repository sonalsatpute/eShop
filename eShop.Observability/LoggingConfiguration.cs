using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace eShop.Observability;

internal interface ILoggingConfiguration
{
    void Configure(IOpenTelemetryBuilder builder);
    
    IServiceCollection Configure(
        IServiceCollection services,
        ResourceBuilder resource);
}

class LoggingConfiguration : ILoggingConfiguration
{
    private readonly IObservabilityOptions _options;

    public LoggingConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }
    
    
    public void Configure(IOpenTelemetryBuilder builder)
    {
        if (!_options.IsLoggingEnabled) return;
            
        builder.WithLogging(logging =>
        {
            logging
                .AddConsoleExporter() // Add Console exporter for development
                .AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);
        });
    }

    public IServiceCollection Configure(
        IServiceCollection services,
        ResourceBuilder resource)
    {
        if (!_options.IsLoggingEnabled) return services;
        
        return services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource);
                options.IncludeScopes = true;
                options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = _options.CollectorEndpoint);
            });
        });
    }
}