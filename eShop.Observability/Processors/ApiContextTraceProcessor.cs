// using System.Diagnostics;
// using Microsoft.AspNetCore.Http;
// using OpenTelemetry;
//
// namespace eShop.Observability.Processors;
//
// public class ApiContextTraceProcessor : BaseProcessor<Activity>
// {
//     private readonly IHttpContextAccessor _httpContextAccessor;
//
//     public ApiContextTraceProcessor(
//         IHttpContextAccessor httpContextAccessor
//         )
//     {
//         _httpContextAccessor = httpContextAccessor;
//     }
//
//     public override void OnEnd(Activity activity)
//     {
//         // HttpContext httpContext = _httpContextAccessor.HttpContext!;
//         // if (httpContext == null)
//         // {
//         //     base.OnEnd(activity);
//         //     return;
//         // }
//         //
//         // CaptureRequest(activity, httpContext.Request);
//         // CaptureResponse(activity, httpContext.Response);
//         
//         base.OnEnd(activity!);
//     }
//
//     private void CaptureRequest(Activity activity, HttpRequest request)
//     {
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.TENANT_ID);
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.SITE_ID);
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.INITIATING_APPID);
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.INSTANCE_ID);
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.LOCALE_CODE);
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.CURRENCY_CODE);
//         CaptureHeader(activity, request?.Headers!, ObservabilityConstants.CORRELATION_ID);
//     }
//     
//     private void CaptureResponse(Activity activity, HttpResponse response)
//     {
//         CaptureHeader(activity, response?.Headers!, ObservabilityConstants.CORRELATION_ID);
//     }
//
//     private void CaptureHeader(Activity activity, IHeaderDictionary headers, string key, bool isRequest = false)
//     {
//         if (activity == null || headers == null) return;
//         
//         string value = GetHeaderValue(headers, key);
//         if (string.IsNullOrEmpty(value)) return;
//         string name = isRequest ? $"request.{key}" : $"response.{key}";
//         
//         activity.SetTag(name, value);
//     }
//     
//     private string GetHeaderValue(IHeaderDictionary headers, string headerName) => 
//         headers.TryGetValue(headerName, out var value) 
//             ? Convert.ToString(value.FirstOrDefault()) ?? string.Empty
//             : string.Empty;
//
// }