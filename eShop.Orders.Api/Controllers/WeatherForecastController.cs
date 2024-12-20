using Microsoft.AspNetCore.Mvc;

namespace eShop.Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "")]
    public IEnumerable<WeatherForecast> Get()
    {
        
        Console.WriteLine($"OTEL_SERVICE_NAME: {Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")}");
        Console.WriteLine($"OTEL_EXPORTER_OTLP_ENDPOINT: {Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")}");
        Console.WriteLine($"OTEL_METRICS_EXPORTER: {Environment.GetEnvironmentVariable("OTEL_METRICS_EXPORTER")}");
        
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet("greeting/{name}")]
    public async Task<string> Greet(string name)
    {
        Console.WriteLine($"OTEL_SERVICE_NAME: {Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")}");
        Console.WriteLine($"OTEL_EXPORTER_OTLP_ENDPOINT: {Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")}");
        Console.WriteLine($"OTEL_METRICS_EXPORTER: {Environment.GetEnvironmentVariable("OTEL_METRICS_EXPORTER")}");
        
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
        HttpResponseMessage response = await client.GetAsync("posts/1");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}