namespace eShop.Observability.Constants;

public static class MetricConstants
{
    public const string ORDERS_CREATED = "orders.created";
    public const string ORDERS_SUBMITTED = "orders.submitted";
    public const string ORDERS_CANCELLED = "orders.cancelled.meter";
    
    public const string PAYMENT_AUTHORIZED = "payment.authorized";
    public const string PAYMENT_CAPTURED = "payment.captured";
    public const string PAYMENT_REFUNDED = "payment.refunded";
}