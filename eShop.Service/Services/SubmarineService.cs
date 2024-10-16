﻿using System.Diagnostics;
using eShop.Observability;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.Service.Services;

/// <summary>
/// Shipping Service 
/// </summary>
public class SubmarineService : IHostedService
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
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using Activity? activity = _observability.CurrentActivitySource.StartActivity();
        
        _logger.LogInformation($"starting {nameof(SubmarineService)}");
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1", cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation($"API response : {content.Length}");
        }
        
        _logger.LogInformation($"API call status code: {response.StatusCode}");
        activity?.SetTag("status.code", response.StatusCode.ToString());
        activity?.AddEvent(new ActivityEvent("Finished API call"));
        activity?.Stop();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"stopping {nameof(SubmarineService)}");
        return Task.CompletedTask;
    }
}