using System.Diagnostics;
using eShop.Observability;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.Service.Services;

/// <summary>
/// Shipping Service 
/// </summary>
public class SubmarineService : IHostedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IObservabilityOptions _observability;
    private readonly IApplicationMetric _metric;
    private readonly ILogger<SubmarineService> _logger;
    private Timer _timer = null!;

    public SubmarineService(IHttpClientFactory httpClientFactory,
        IObservabilityOptions observability,
        IApplicationMetric metric,
        ILogger<SubmarineService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _observability = observability;
        _metric = metric;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using Activity? activity = _observability?.CurrentActivitySource?.StartActivity();
        activity?.AddEvent(new ActivityEvent("calling api endpoint"));
        // Make an HTTP GET call
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts", cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation($"API response : {content.Length}");
        }

        _logger.LogInformation($"API call status code: {response.StatusCode}");
        activity?.SetTag("status.code", response.StatusCode.ToString());
        activity?.AddEvent(new ActivityEvent("Finished API call"));
        activity?.Stop();
        
        // Start a timer to keep the service running
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    private void DoWork(object? state)
    {
        using Activity? activity = _observability?.CurrentActivitySource.StartActivity();
        activity?.AddEvent(new ActivityEvent("working on something"));
        activity?.SetTag("worker", "SubmarineService");

        _metric.OrderCreated("fake_tenant_id", "fake_site_id", 2);
        _metric.OrderSubmitted("fake_tenant_id", "fake_site_id", 1);
        
        _metric.PaymentAuthorized("fake_tenant_id", "fake_site_id", 1);
        _metric.PaymentCaptured("fake_tenant_id", "fake_site_id", 1);
        
        activity?.Stop();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}