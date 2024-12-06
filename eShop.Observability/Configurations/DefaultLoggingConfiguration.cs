using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace eShop.Observability.Configurations;

internal interface ILoggingConfiguration
{
    void Configure(IOpenTelemetryBuilder builder,
        Action<LoggerProviderBuilder> configureLoggerProvider);
    
    IServiceCollection Configure(IServiceCollection services,
        ResourceBuilder resource, 
        Action<LoggerProviderBuilder> configureLoggerProvider);
}

internal class DefaultLoggingConfiguration : ILoggingConfiguration
{
    private readonly IObservabilityOptions _options;

    public DefaultLoggingConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }
    
    public void Configure(IOpenTelemetryBuilder builder,
        Action<LoggerProviderBuilder> configureLoggerProvider)
    {
        if (!_options.IsLoggingEnabled) return;

        if (configureLoggerProvider != null)
            builder.WithLogging(configureLoggerProvider);
        else
            builder.WithLogging(logging =>
            {
                logging.AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);
                
                if (_options.ExportToConsole) logging.AddConsoleExporter();
            });
    }

    public IServiceCollection Configure(IServiceCollection services,
        ResourceBuilder resource, 
        Action<LoggerProviderBuilder> configureLoggerProvider)
    {
        if (!_options.IsLoggingEnabled) return services;
        
        if (configureLoggerProvider != null)
            throw new NotImplementedException("Custom LoggerProviderBuilder configuration for non web app/api is not supported yet.");
        
        return services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource);
                options.IncludeScopes = true;
                options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = _options.CollectorEndpoint);
                
                if (_options.ExportToConsole) options.AddConsoleExporter();
            });
        });
    }
}