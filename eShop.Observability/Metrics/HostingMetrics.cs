using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;

namespace eShop.Observability.Metrics;

public sealed class HostingMetrics : IDisposable
{
    public const string MeterName = "Microsoft.AspNetCore.Hosting";

    private readonly Meter _meter;
    private readonly UpDownCounter<long> _activeRequestsCounter;
    private readonly Histogram<double> _requestDuration;
    
    public HostingMetrics()
    {
        // IMeterFactory meterFactory
        _meter = new Meter(MeterName);

        _activeRequestsCounter = _meter.CreateUpDownCounter<long>(
            "http.server.active_requests",
            unit: "{request}",
            description: "Number of active HTTP server requests.");

        _requestDuration = _meter.CreateHistogram<double>(
            "http.server.request.duration",
            unit: "s",
            description: "Duration of HTTP server requests.");
            
        // var advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = MetricsConstants.ShortSecondsBucketBoundaries }
    }

    // Note: Calling code checks whether counter is enabled.
    public void RequestStart(
        string scheme, 
        string method, 
        TagList customTags)
    {
        // Tags must match request end.
        TagList tags = new TagList();
        InitializeRequestTags(ref tags, scheme, method);
        foreach (KeyValuePair<string, object?> tag in customTags)
        {
            tags.Add(tag);
        }
        _activeRequestsCounter.Add(1, tags);
    }

    public void RequestEnd(
        string protocol, 
        string scheme, 
        string method, 
        string? route, 
        int statusCode, 
        bool unhandledRequest, 
        Exception? exception, 
        TagList customTags, 
        double duration, 
        bool disableHttpRequestDurationMetric)
    {
        TagList tags = new TagList();
        // Add before some built in tags so custom tags are prioritized when dealing with duplicates.
        foreach (KeyValuePair<string, object?> tag in customTags)
        {
            tags.Add(tag);
        }
        
        InitializeRequestTags(ref tags, scheme, method);

        // Tags must match request start.
        if (_activeRequestsCounter.Enabled)
        {
            _activeRequestsCounter.Add(-1, tags);
        }

        if (!disableHttpRequestDurationMetric && _requestDuration.Enabled)
        {
            if (TryGetHttpVersion(protocol, out var httpVersion))
            {
                tags.Add("network.protocol.version", httpVersion);
            }
            if (unhandledRequest)
            {
                tags.Add("aspnetcore.request.is_unhandled", true);
            }

            // Add information gathered during request.
            tags.Add("http.response.status_code", GetBoxedStatusCode(statusCode));
            if (route != null)
            {
                tags.Add("http.route", route);
            }

            // This exception is only present if there is an unhandled exception.
            // An exception caught by ExceptionHandlerMiddleware and DeveloperExceptionMiddleware isn't thrown to here. Instead, those middleware add error.type to custom tags.
            if (exception != null)
            {
                // Exception tag could have been added by middleware. If an exception is later thrown in request pipeline
                // then we don't want to add a duplicate tag here because that breaks some metrics systems.
                tags.Add("error.type", exception.GetType().FullName);
            }

            _requestDuration.Record(duration, tags);
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
    }

    public bool IsEnabled() => _activeRequestsCounter.Enabled || _requestDuration.Enabled;

    private static void InitializeRequestTags(ref TagList tags, string scheme, string method)
    {
        tags.Add("url.scheme", scheme);
        tags.Add("http.request.method", ResolveHttpMethod(method));
    }

    private static readonly object[] BoxedStatusCodes = new object[512];

    private static object GetBoxedStatusCode(int statusCode)
    {
        object[] boxes = BoxedStatusCodes;
        return (uint)statusCode < (uint)boxes.Length
            ? boxes[statusCode] ??= statusCode
            : statusCode;
    }

    private static readonly HashSet<string> KnownMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Connect,
        HttpMethods.Delete,
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Patch,
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Trace
    };

    private static string ResolveHttpMethod(string method) => KnownMethods.Contains(method) ? method : "_OTHER";

    private static bool TryGetHttpVersion(string protocol, [NotNullWhen(true)] out string? version)
    {
        if (HttpProtocol.IsHttp11(protocol))
        {
            version = "1.1";
            return true;
        }
        if (HttpProtocol.IsHttp2(protocol))
        {
            // HTTP/2 only has one version.
            version = "2";
            return true;
        }
        if (HttpProtocol.IsHttp3(protocol))
        {
            // HTTP/3 only has one version.
            version = "3";
            return true;
        }
        if (HttpProtocol.IsHttp10(protocol))
        {
            version = "1.0";
            return true;
        }
        if (HttpProtocol.IsHttp09(protocol))
        {
            version = "0.9";
            return true;
        }
        version = null;
        return false;
    }
}