using System;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace eShop.Observability.Configurations;

internal interface IMetricsConfiguration
{
    void Configure(IOpenTelemetryBuilder builder, Action<MeterProviderBuilder> configureMeterProvider);
    void Configure(ResourceBuilder resource, Action<MeterProviderBuilder> configureMeterProvider);
}

internal class DefaultMetricsConfiguration : IMetricsConfiguration
{
    private readonly IObservabilityOptions _options;
    
    private const string HOSTING_METER = "Microsoft.AspNetCore.Hosting";
    private const string KESTREL_METER = "Microsoft.AspNetCore.Server.Kestrel";
    private const string MASS_TRANSIT_METER = "MassTransit"; // Out of the box support is available after MassTransit v8+

    public DefaultMetricsConfiguration(IObservabilityOptions options)
    {
        _options = options;
    }
    
    /// <summary>
    /// Configure Metrics for Web App
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureMeterProvider"></param>
    public void Configure(IOpenTelemetryBuilder builder,
        Action<MeterProviderBuilder> configureMeterProvider)
    {
        if (!_options.IsMetricsEnabled) return;

        builder.WithMetrics(configureMeterProvider ?? ConfigureMetrics);
    }

    /// <summary>
    /// Configure Metrics for Console App
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="configureMeterProvider"></param>
    public void Configure(ResourceBuilder resource,
        Action<MeterProviderBuilder> configureMeterProvider)
    {
        if (!_options.IsMetricsEnabled) return;
        
        MeterProviderBuilder builder = Sdk.CreateMeterProviderBuilder();
        builder.SetResourceBuilder(resource);
        
        if (configureMeterProvider != null)
            configureMeterProvider.Invoke(builder);
        else
            ConfigureMetrics(builder);
    }

    private void ConfigureMetrics(MeterProviderBuilder builder)
    {
        if (_options.ForWebApp)
        {
            // Metrics provides
            builder
                .AddMeter(HOSTING_METER)
                .AddMeter(KESTREL_METER); // Full Support in .NET 8 and later

            builder.AddAspNetCoreInstrumentation();
        }

        builder.AddMeter(MASS_TRANSIT_METER); // Support for MassTransit v8+

        foreach (string meterName in _options.MeterNames)
        {
            builder.AddMeter(meterName);
        }

        builder
            .AddHttpClientInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(options => options.Endpoint = _options.CollectorEndpoint);
        
        if (_options.ExportToConsole) builder.AddConsoleExporter();

        if (_options.ForConsoleApp) builder.Build();
    }
}