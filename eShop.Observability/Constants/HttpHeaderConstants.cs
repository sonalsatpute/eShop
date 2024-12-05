namespace eShop.Observability.Constants;

public static class HttpHeaderConstants
{
    public const string TENANT_ID = "x-vol-tenant";
    public const string SITE_ID = "x-vol-site";
    
    public const string LOCALE_CODE = "x-vol-locale";
    public const string CURRENCY_CODE = "x-vol-currency";
    public const string CATALOG_ID = "x-vol-catalog";
    public const string MASTER_CATALOG_ID = "x-vol-master-catalog";
    
    public const string CORRELATION_ID = "x-vol-correlation-id";
    public const string INSTANCE_ID = "x-vol-instance";
    public const string REFERER = "referer";
    public const string INITIATING_APPID = "x-vol-initiating-app";
}