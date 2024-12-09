using System.Diagnostics;
using eShop.Observability.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eShop.Observability.Middlewares;

public class HostingMetricsMiddlewares
{
    private readonly RequestDelegate _next;
    
    private readonly IObservabilityOptions _options;
    private readonly HostingMetrics _hostingMetrics;

    public HostingMetricsMiddlewares(
        RequestDelegate next, 
        IObservabilityOptions options,
        HostingMetrics hostingMetrics)
    {
        _next = next;
        _options = options;
        _hostingMetrics = hostingMetrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.IsMetricsEnabled)
        {
            await _next(context);
            return;
        }
        
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            _hostingMetrics.RequestStart(
                context.Request.Scheme, 
                context.Request.Method,
                _options.EnvironmentTags);
        
            context.Response.OnStarting(() => RequestEnd(context, stopwatch));
        
            await _next(context);
        }
        catch (Exception exception)
        {
            await RequestEnd(context, stopwatch, exception);
            throw;
        }
        
    }

    private Task RequestEnd(HttpContext context, Stopwatch stopwatch, Exception? exception = null)
    {
        stopwatch.Stop();

        string? routeTemplate = null;
        
        if (context.GetEndpoint() is RouteEndpoint endpoint)
        {
            routeTemplate = endpoint.RoutePattern.RawText;
        }
        
        _hostingMetrics.RequestEnd(
            context.Request.Protocol, 
            context.Request.Scheme, 
            context.Request.Method, 
            routeTemplate, 
            context.Response.StatusCode,
            false, 
            exception, 
            _options.EnvironmentTags, 
            stopwatch.Elapsed.TotalSeconds,
            false);
            
        return Task.CompletedTask;
    }
}