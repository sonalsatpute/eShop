using System.Diagnostics;
using eShop.Observability.DaemonService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace eShop.Observability.DaemonService;

public class ObservabilityStartupFilter : IStartupFilter, IHostStartupFilter
{
    private readonly IObservabilityOptions _options;

    public ObservabilityStartupFilter(IObservabilityOptions options)
    {
        _options = options;
    }
    
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Call the next delegate/middleware in the pipeline
            Activity? activity = _options.CurrentActivitySource.StartActivity();
            activity?.AddEvent(new ActivityEvent("ObservabilityStartupFilter.Configure"));
            activity?.Stop();
            next(app);
        };
    }

    public void Configure(IHost host)
    {
        // Call the next delegate/middleware in the pipeline
    }
}
