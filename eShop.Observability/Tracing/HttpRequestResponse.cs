using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using eShop.Observability.Constants;
using OpenTelemetry.Trace;

namespace eShop.Observability.Tracing;

public static class HttpRequestResponse
{
    
    /// <summary>
    /// map of HTTP_HEADER_NAME and OTLP_ATTRIBUTE_NAME
    /// </summary>
    private static readonly IDictionary<string, string> HeadersMap = new Dictionary<string, string>()
    {
        //Can't use eShop.Api.Contracts.Header due to circular dependency
        { HttpHeaderConstants.TENANT_ID, ObservabilityConstants.TENANT_ID },
        { HttpHeaderConstants.SITE_ID, ObservabilityConstants.SITE_ID },
        
        { HttpHeaderConstants.LOCALE_CODE, ObservabilityConstants.LOCALE_CODE},
        { HttpHeaderConstants.CURRENCY_CODE, ObservabilityConstants.CURRENCY_CODE},
        { HttpHeaderConstants.CATALOG_ID, ObservabilityConstants.CATALOG_ID},
        { HttpHeaderConstants.MASTER_CATALOG_ID, ObservabilityConstants.MASTER_CATALOG_ID},
        
        { HttpHeaderConstants.CORRELATION_ID, ObservabilityConstants.CORRELATION_ID},
        { HttpHeaderConstants.INSTANCE_ID, ObservabilityConstants.INSTANCE_ID},
        { HttpHeaderConstants.REFERER, ObservabilityConstants.REFERER}
    };
    
    public static void EnrichWithHttpRequestMessage(Activity activity, HttpRequestMessage request)
    {
        try
        {
            if (request?.RequestUri?.Query.Length > 0)
            {
                Dictionary<string, string> queryParams = request.RequestUri.Query
                    .TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Select(q => q.Split('=', 2, StringSplitOptions.RemoveEmptyEntries))
                    .Where(q => q.Length == 2)
                    .ToDictionary(q => q[0], q => q[1]);
            
                foreach (KeyValuePair<string, string> param in queryParams) 
                    activity.SetTag($"{ObservabilityConstants.URL_QUERY}.{param.Key}", param.Value);
            }

            EnrichWithHttpRequestMessage(activity, request?.Headers!, true);
        }
        catch (Exception e)
        {
            activity.RecordException(e);
        }
    }
    
    public static void EnrichWithHttpResponseMessage(Activity activity, HttpResponseMessage response)
    {
        try
        {
            EnrichWithHttpRequestMessage(activity, response?.Headers!, false);
        }
        catch (Exception e)
        {
            activity.RecordException(e);
        }
        
    }


    public static void EnrichWithHttpRequest(Activity activity, HttpRequest request)
    {
        try
        {
            if (request == null) return;

            if (request.QueryString.HasValue)
            {
                foreach (KeyValuePair<string, StringValues> param in request.Query)
                {
                    activity.SetTag($"{ObservabilityConstants.URL_QUERY}.{param.Key}", param.Value);
                }
            }

            EnrichWithHttpRequestMessage(activity, request.Headers, true);
        }
        catch (Exception e)
        {
            activity.RecordException(e);
        }
    }
    
    public static void EnrichWithHttpResponse(Activity activity, HttpResponse response)
    {
        try
        {
            EnrichWithHttpRequestMessage(activity, response.Headers, false);
        }
        catch (Exception e)
        {
            activity.RecordException(e);
        }
    }

    private static void EnrichWithHttpRequestMessage(Activity activity, IHeaderDictionary headers, bool isRequest)
    {
        SetTag(activity, headers, HttpHeaderConstants.TENANT_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.SITE_ID, isRequest);

        SetTag(activity, headers, HttpHeaderConstants.LOCALE_CODE, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.CURRENCY_CODE, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.CATALOG_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.MASTER_CATALOG_ID, isRequest);
        
        SetTag(activity, headers, HttpHeaderConstants.CORRELATION_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.INSTANCE_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.REFERER, isRequest);
    }

    private static void EnrichWithHttpRequestMessage(Activity activity, HttpHeaders headers, bool isRequest)
    {
        SetTag(activity, headers, HttpHeaderConstants.TENANT_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.SITE_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.INITIATING_APPID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.INSTANCE_ID, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.LOCALE_CODE, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.CURRENCY_CODE, isRequest);
        SetTag(activity, headers, HttpHeaderConstants.CORRELATION_ID, isRequest);
    }

    private static void SetTag(Activity activity, HttpHeaders headers, string key, bool isRequest = false)
    {
        if (activity == null || headers == null) return;
        
        SetTag(activity, key, GetHeaderValue(headers, key), isRequest);
    }
    
    private static void SetTag(Activity activity, IHeaderDictionary headers, string key, bool isRequest = false)
    {
        if (activity == null || headers == null) return;
        
        SetTag(activity, key, GetHeaderValue(headers, key), isRequest);
    }

    private static void SetTag(Activity activity, string key, string value, bool isRequest = false)
    {
        if (string.IsNullOrEmpty(value)) return;
        if (!HeadersMap.ContainsKey(key)) return;
        
        string name = isRequest 
            ? $"{ObservabilityConstants.HTTP_REQUEST_HEADER}.{HeadersMap[key]}" 
            : $"{ObservabilityConstants.HTTP_RESPONSE_HEADER}.{HeadersMap[key]}";
        
        activity.SetTag(name, value);
    }
    
    private static string GetHeaderValue(HttpHeaders headers, string headerName) =>
        headers.TryGetValues(headerName, out var value)
            ? Convert.ToString(value.FirstOrDefault()) ?? string.Empty
            : string.Empty;
    
    private static string GetHeaderValue(IHeaderDictionary headers, string headerName) =>
        headers.TryGetValue(headerName, out var value)
            ? Convert.ToString(value.FirstOrDefault()) ?? string.Empty
            : string.Empty;
}