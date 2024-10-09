using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace eShop.Observability;

internal interface IMetricsConfiguration
{
    void Configure(OpenTelemetryBuilder builder);
    void Configure(ResourceBuilder resource);
}

internal class MetricsConfiguration : IMetricsConfiguration
{
    private readonly IObservabilityOptions _options;
    
    private const string HOSTING_METER = "Microsoft.AspNetCore.Hosting";
    private const string KESTREL_METER = "Microsoft.AspNetCore.Server.Kestrel";
    private const string MASS_TRANSIT_METER = "MassTransit"; // Out of the box support is available after MassTransit v8+

    public MetricsConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }
    
    /// <summary>
    /// Configure Metrics for Web App
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(OpenTelemetryBuilder builder)
    {
        if (!_options.IsMetricsEnabled) return;
        
        builder.WithMetrics(metrics =>  ConfigureMetrics(metrics));
    }

    /// <summary>
    /// Configure Metrics for Console App
    /// </summary>
    /// <param name="resource"></param>
    public void Configure(ResourceBuilder resource)
    {
        if (!_options.IsMetricsEnabled) return;
        
        MeterProviderBuilder metrics = Sdk.CreateMeterProviderBuilder();
        metrics.SetResourceBuilder(resource);
        
        ConfigureMetrics(metrics, true);
    }

    private void ConfigureMetrics(MeterProviderBuilder metrics, bool isConsoleApp = false)
    {
        MeterProviderBuilder builder = metrics.AddAspNetCoreInstrumentation();
        if (!isConsoleApp)
        {
            // Metrics provides
            builder
                .AddMeter(HOSTING_METER)
                .AddMeter(KESTREL_METER); // Full Support in .NET 8 and later
        }

        builder.AddMeter(MASS_TRANSIT_METER);

        metrics
            .AddHttpClientInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddConsoleExporter() // Add Console exporter for development
            .AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);

        if (isConsoleApp)
        {
            metrics.Build();
        }
    }
}