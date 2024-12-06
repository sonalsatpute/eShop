using eShop.Observability.Constants;

namespace eShop.Observability;

public interface IApplicationMetric
{
    void OrderCreated(string tenantId, string siteId, long count = 1);
    void OrderSubmitted(string tenantId, string siteId, long count = 1);
    void OrderCancelled(string tenantId, string siteId, long count = 1);
    
    void PaymentAuthorized(string tenantId, string siteId, long count = 1);
    void PaymentCaptured(string tenantId, string siteId, long count = 1);
    void PaymentRefunded(string tenantId, string siteId, long count = 1);
    
}

public class ApplicationMetric : IApplicationMetric
{
    private readonly MeterCounter _ordersCreated = new(MetricConstants.ORDERS_CREATED);
    private readonly MeterCounter _ordersSubmitted = new(MetricConstants.ORDERS_SUBMITTED);
    private readonly MeterCounter _ordersCancelled = new(MetricConstants.ORDERS_CANCELLED);
    
    private readonly MeterCounter _paymentAuthorized = new(MetricConstants.PAYMENT_AUTHORIZED);
    private readonly MeterCounter _paymentCaptured = new(MetricConstants.PAYMENT_CAPTURED);
    private readonly MeterCounter _paymentRefunded = new(MetricConstants.PAYMENT_REFUNDED);

    private readonly List<KeyValuePair<string, object>> _environmentTags;
    
    public ApplicationMetric(IObservabilityOptions options)
    {
        _environmentTags = new List<KeyValuePair<string, object>>
        {
            new(ObservabilityConstants.SERVICE_NAME, options.ServiceName),
            new(ObservabilityConstants.SERVICE_VERSION, options.ServiceVersion),
            new(ObservabilityConstants.HOST_NAME, options.HostName),
            new(ObservabilityConstants.ENVIRONMENT, options.Environment),
            new(ObservabilityConstants.SCALE_UNIT_ID, options.ScaleUnitId),
            new(ObservabilityConstants.NAMESPACE_NAME, $"{options.Environment}-{options.ScaleUnitId}")
        };
    }
    
    public void OrderCreated(string tenantId, string siteId, long count = 1)
    {
       _ordersCreated.Increment(CreateTags(tenantId, siteId), count);
    }

    public void OrderSubmitted(string tenantId, string siteId, long count = 1)
    {
        _ordersSubmitted.Increment(CreateTags(tenantId, siteId), count);
    }

    public void OrderCancelled(string tenantId, string siteId, long count = 1)
    {
        _ordersCancelled.Increment(CreateTags(tenantId, siteId), count);
    }

    public void PaymentAuthorized(string tenantId, string siteId, long count = 1)
    {
        _paymentAuthorized.Increment(CreateTags(tenantId, siteId), count);
    }

    public void PaymentCaptured(string tenantId, string siteId, long count = 1)
    {
        _paymentCaptured.Increment(CreateTags(tenantId, siteId), count);
    }

    public void PaymentRefunded(string tenantId, string siteId, long count = 1)
    {
        _paymentRefunded.Increment(CreateTags(tenantId, siteId), count);
    }
    
    private ReadOnlySpan<KeyValuePair<string, object>> CreateTags(string tenantId, string siteId)
    {
        List<KeyValuePair<string, object>> pairs = new()
        {
            new(ObservabilityConstants.TENANT_ID, tenantId),
            new(ObservabilityConstants.SITE_ID, siteId)
        };
        pairs.AddRange(_environmentTags);
        return pairs.ToArray();
    }
}