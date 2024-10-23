using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace eShop.Observability;

internal interface ILoggingConfiguration
{
    void Configure(IOpenTelemetryBuilder builder,
    Action<LoggerProviderBuilder>? configureLoggerProvider);
    
    IServiceCollection Configure(IServiceCollection services,
        ResourceBuilder resource, 
        Action<LoggerProviderBuilder>? configureLoggerProvider);
}

class LoggingConfiguration : ILoggingConfiguration
{
    private readonly IObservabilityOptions _options;

    public LoggingConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }

    public void Configure(IOpenTelemetryBuilder builder,
        Action<LoggerProviderBuilder>? configureLoggerProvider)
    {
        if (!_options.IsLoggingEnabled) return;

        if (configureLoggerProvider != null)
            builder.WithLogging(configureLoggerProvider);
        else
            builder.WithLogging(logging =>
            {
                logging
                    .AddConsoleExporter() // Add Console exporter for development
                    .AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);
            });
    }

    public IServiceCollection Configure(IServiceCollection services,
        ResourceBuilder resource, 
        Action<LoggerProviderBuilder>? configureLoggerProvider)
    {
        if (!_options.IsLoggingEnabled) return services;
        
        if (configureLoggerProvider != null)
            throw new NotImplementedException("Custom LoggerProviderBuilder configuration is not supported yet.");
        
        return services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource);
                options.IncludeScopes = true;
                options.AddConsoleExporter(); // Add Console exporter for development
                options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = _options.CollectorEndpoint);
            });
        });
    }
}