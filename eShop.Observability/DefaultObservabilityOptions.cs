using System.Diagnostics;
using System.Reflection;
using eShop.Observability.Constants;
using Microsoft.Extensions.Configuration;

namespace eShop.Observability;

/// <summary>
/// OpenTelemetry Observability Configuration
/// </summary>
public interface IObservabilityOptions
{
    ActivitySource CurrentActivitySource { get; }

    string Environment { get; }
    string ScaleUnitId { get; }
    
    string ActivitySourceName => ServiceName;
    string HostName { get; }
    string ServiceName { get; }
    string ServiceVersion { get; } 
    
    Uri CollectorEndpoint { get; }
    
    bool IsTracingEnabled { get; }
    bool IsMetricsEnabled { get; }
    bool IsLoggingEnabled { get; }
    bool IsObservabilityEnabled { get; }
    bool ForWebApp { get; }
    bool ForConsoleApp => !ForWebApp;
    bool ExportToConsole { get; }

    IEnumerable<string> MeterNames { get; }

    TagList EnvironmentTags { get; }
}

public class DefaultObservabilityOptions : IObservabilityOptions
{
    const string OPEN_TELEMETRY_COLLECTOR_URL   = "open_telemetry_collector_url";
    const string OPEN_TELEMETRY_TRACING_ENABLED = "open_telemetry_tracing_enabled";
    const string OPEN_TELEMETRY_METRICS_ENABLED = "open_telemetry_metrics_enabled";
    const string OPEN_TELEMETRY_LOGGING_ENABLED = "open_telemetry_logging_enabled";
    const string OPEN_TELEMETRY_CONSOLE_EXPORTER_ENABLED = "open_telemetry_console_exporter_enabled";
    
    const string ENVIRONMENT = "Environment";
    const string SCALE_UNIT_ID = "ScaleUnitId";

    public DefaultObservabilityOptions(
        IConfiguration settings, 
        IServiceInfoProvider serviceInfoProvider,
        bool forWebApp
        )
    {
        ForWebApp = forWebApp;
        HostName = serviceInfoProvider.GetHostName();
        
        ServiceName = serviceInfoProvider.GetServiceName(settings);
        ServiceVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
        CurrentActivitySource = new ActivitySource(ServiceName, ServiceVersion);
        
        Environment = settings.GetValue(ENVIRONMENT, string.Empty)!;
        ScaleUnitId = settings.GetValue(SCALE_UNIT_ID, string.Empty)!;
        
        IsTracingEnabled = settings.GetValue(OPEN_TELEMETRY_TRACING_ENABLED, false);
        IsMetricsEnabled = settings.GetValue(OPEN_TELEMETRY_METRICS_ENABLED, false);
        IsLoggingEnabled = settings.GetValue(OPEN_TELEMETRY_LOGGING_ENABLED, false);
        
        ExportToConsole = settings.GetValue(OPEN_TELEMETRY_CONSOLE_EXPORTER_ENABLED, false);
        
        string url = settings.GetValue(OPEN_TELEMETRY_COLLECTOR_URL, "http://localhost:4317")!;
        //fault if enabled but no url
        CollectorEndpoint = new Uri(url);
        
        EnvironmentTags = new()
        {
            { ObservabilityConstants.ENVIRONMENT, Environment },
            { ObservabilityConstants.SERVICE_NAME, ServiceName },
            { ObservabilityConstants.SERVICE_VERSION, ServiceVersion },
            { ObservabilityConstants.HOST_NAME, HostName },
            { ObservabilityConstants.SCALE_UNIT_ID, ScaleUnitId },
            { ObservabilityConstants.NAMESPACE_NAME, $"{Environment}-{ScaleUnitId}" }
        };
    }

    public bool ExportToConsole { get; }
    public bool ForWebApp { get; }

    public TagList EnvironmentTags { get; }
    
    public IEnumerable<string> MeterNames => new[]
    {
        MetricConstants.ORDERS_CREATED,
        MetricConstants.ORDERS_SUBMITTED,
        MetricConstants.ORDERS_CANCELLED,
        
        MetricConstants.PAYMENT_AUTHORIZED,
        MetricConstants.PAYMENT_CAPTURED,
        MetricConstants.PAYMENT_REFUNDED
    };
    
    public ActivitySource CurrentActivitySource { get;}
    
    public string Environment { get; }
    public string ScaleUnitId { get; }
    public string HostName { get; }
    public string ServiceName { get; }
    public string ServiceVersion { get; }
    public Uri CollectorEndpoint { get; }
    public bool IsTracingEnabled { get; }
    public bool IsMetricsEnabled { get; }
    public bool IsLoggingEnabled { get; }
    public bool IsObservabilityEnabled => IsTracingEnabled || IsMetricsEnabled || IsLoggingEnabled;
}