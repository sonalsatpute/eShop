using System.Diagnostics;
using eShop.Observability;
using eShop.Observability.Constants;
using Microsoft.Extensions.Configuration;

public class CustomObservabilityOptions : IObservabilityOptions
{
    const string SERVICE_NAME = "SERVICE_NAME";
    const string ENVIRONMENT = "Environment";
    const string SCALE_UNIT_ID = "ScaleUnitId";
    
    const string OPEN_TELEMETRY_COLLECTOR_URL = "open_telemetry_collector_url";
    const string OPEN_TELEMETRY_TRACING_ENABLED = "open_telemetry_tracing_enabled";
    const string OPEN_TELEMETRY_METRICS_ENABLED = "open_telemetry_metrics_enabled";
    const string OPEN_TELEMETRY_LOGGING_ENABLED = "open_telemetry_logging_enabled";
    const string OPEN_TELEMETRY_CONSOLE_EXPORTER_ENABLED = "open_telemetry_console_exporter_enabled";
    public CustomObservabilityOptions(IConfiguration settings, bool forWebApp)
    {
        ForWebApp = forWebApp;
        HostName = "development-host-machine";
        ServiceName = settings.GetValue<string>(SERVICE_NAME, "missing-service-name") + "-custom";
        ServiceVersion = "9.9.9.9";
        
        Environment = settings.GetValue<string>(ENVIRONMENT, string.Empty)!;
        ScaleUnitId = settings.GetValue<string>(SCALE_UNIT_ID, string.Empty)!;
        
        CurrentActivitySource = new ActivitySource(ServiceName, ServiceVersion);
        IsTracingEnabled = settings.GetValue(OPEN_TELEMETRY_TRACING_ENABLED, false);
        IsMetricsEnabled = settings.GetValue(OPEN_TELEMETRY_METRICS_ENABLED, false);
        IsLoggingEnabled = settings.GetValue(OPEN_TELEMETRY_LOGGING_ENABLED, false);
        ExportToConsole = settings.GetValue(OPEN_TELEMETRY_CONSOLE_EXPORTER_ENABLED, false);    
        
        string url = settings.GetValue<string>(OPEN_TELEMETRY_COLLECTOR_URL, "http://localhost:4317")!;
        //todo: fault if enabled but no url
        CollectorEndpoint = new Uri(url);
        
        EnvironmentTags = new TagList
        {
            { "environment", Environment },
            { "scale_unit_id", ScaleUnitId }
        };
    }

    

    public bool ForWebApp { get; }
    public bool ExportToConsole { get; }

    public IEnumerable<string> MeterNames => new[]
    {
        MetricConstants.ORDERS_CREATED,
        MetricConstants.ORDERS_SUBMITTED,
        MetricConstants.ORDERS_CANCELLED,
        
        MetricConstants.PAYMENT_AUTHORIZED,
        MetricConstants.PAYMENT_CAPTURED,
        MetricConstants.PAYMENT_REFUNDED
    };

    public TagList EnvironmentTags { get; }

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