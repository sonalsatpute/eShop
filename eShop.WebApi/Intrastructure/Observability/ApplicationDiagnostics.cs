using System.Diagnostics.Metrics;

namespace eShop.WebApi.Infrastructure.Observability;

public class ApplicationDiagnostics
{
    public const string ServiceName = "eShop.WebApi";
    public static readonly Meter Meter = new(ServiceName);
}