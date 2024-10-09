using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace eShop.Observability;

public interface ICaptureHeader
{
    void SetTags(Activity activity, IHeaderDictionary headers, bool isRequest = false);
    void SetTags(Activity activity, HttpHeaders headers, bool isRequest = false);
}

class CaptureHeader : ICaptureHeader
{
    public void SetTags(Activity activity, IHeaderDictionary headers, bool isRequest = false)
    {
        SetTag(activity, headers, ObservabilityConstants.TENANT_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.SITE_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.INITIATING_APPID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.INSTANCE_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.LOCALE_CODE, isRequest);
        SetTag(activity, headers, ObservabilityConstants.CURRENCY_CODE, isRequest);
        SetTag(activity, headers, ObservabilityConstants.CORRELATION_ID, isRequest);
        
    }

    public void SetTags(Activity activity, HttpHeaders headers, bool isRequest = false)
    {
        SetTag(activity, headers, ObservabilityConstants.TENANT_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.SITE_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.INITIATING_APPID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.INSTANCE_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.LOCALE_CODE, isRequest);
        SetTag(activity, headers, ObservabilityConstants.CURRENCY_CODE, isRequest);
        SetTag(activity, headers, ObservabilityConstants.CORRELATION_ID, isRequest);
        SetTag(activity, headers, ObservabilityConstants.USER_AGENT, isRequest);
    }

    private void SetTag(Activity activity, HttpHeaders headers, string key, bool isRequest = false)
    {
        if (activity == null || headers == null) return;
        
        SetTag(activity, key, GetHeaderValue(headers, key), isRequest);
    }
    
    private void SetTag(Activity activity, IHeaderDictionary headers, string key, bool isRequest = false)
    {
        if (activity == null || headers == null) return;
        
        SetTag(activity, key, GetHeaderValue(headers, key), isRequest);
    }

    private static void SetTag(Activity activity, string key, string value, bool isRequest = false)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        string name = isRequest ? $"{ObservabilityConstants.HTTP}.{ObservabilityConstants.REQUEST}.{ObservabilityConstants.HEADER}.{key}" : $"{ObservabilityConstants.RESPONSE}.{key}";
        activity.SetTag(name, value);
    }
    
    private string GetHeaderValue(HttpHeaders headers, string headerName) =>
        headers.TryGetValues(headerName, out var value)
            ? Convert.ToString(value.FirstOrDefault()) ?? string.Empty
            : string.Empty;
    
    private string GetHeaderValue(IHeaderDictionary headers, string headerName) =>
        headers.TryGetValue(headerName, out var value)
            ? Convert.ToString(value.FirstOrDefault()) ?? string.Empty
            : string.Empty;
}