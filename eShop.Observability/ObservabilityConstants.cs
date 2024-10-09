namespace eShop.Observability;

public static class ObservabilityConstants
{
    public const string TRACE_PARENT = "traceparent";
    
    public const string HTTP = "http";
    public const string REQUEST = "request";
    public const string RESPONSE = "response";
    public const string HEADER = "header";
    public const string IGNORED_HEALTH_ENDPOINT = "/health";
    
    public const string SERVICE_VERSION = "service.version";
    public const string URL_QUERY = "url.query";
    public const string TENANT_ID = "tenant.id";
    public const string SITE_ID = "site.id";
    public const string CORRELATION_ID = "correlation.id";

    public const string ACTION_ID = "action.id";
    public const string ACTION_TIMEOUT = "action.timeout";
    public const string ACTION_ERROR_NAME = "action.error.name";
    public const string ACTION_DOMAIN = "action.domain";
    public const string ACTION_FUNCTION_ID = "action.function.id";
    public const string ACTION_FUNCTION_NAME = "action.function.name";
    public const string ACTION_APP_KEY = "action.app.key";
    public const string INITIATING_APPID = "initiating.appid";
    public const string INSTANCE_ID ="instance.id";
    public const string CATALOG_ID = "catalog.id";
    public const string MASTER_CATALOG_ID="master.catalog.id";
    public const string LOCALE_CODE = "locale.code";
    public const string CURRENCY_CODE = "currency.code";
    
    public const string USER_AGENT = "user-agent";
}