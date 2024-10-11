using System.Diagnostics;
using eShop.Observability;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.Service.Services;

/// <summary>
/// Shipping Service 
/// </summary>
public class SubmarineService : BackgroundService
{

    private readonly IObservabilityOptions _observability;
    private readonly ILogger<SubmarineService> _logger;
    
    public SubmarineService(
        IObservabilityOptions observability,
        ILogger<SubmarineService> logger
        )
    {
        _observability = observability;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using Activity? activity = _observability.CurrentActivitySource.StartActivity();
        
        _logger.LogInformation($"starting {nameof(SubmarineService)}");
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1", stoppingToken);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(stoppingToken);
            _logger.LogInformation($"API response : {content.Length}");
        }
        
        _logger.LogInformation($"API call status code: {response.StatusCode}");
        activity?.SetTag("status.code", response.StatusCode.ToString());
        activity?.AddEvent(new ActivityEvent("Finished API call"));
        activity?.Stop();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"stopping {nameof(SubmarineService)}");
        return Task.CompletedTask;
    }
}