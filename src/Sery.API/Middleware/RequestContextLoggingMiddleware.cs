using System.Diagnostics;
using Serilog.Context;

namespace Sery.API.Middleware;

public sealed class RequestContextLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestContextLoggingMiddleware> logger)
{
    private const string CorrelationHeaderName = "X-Correlation-ID";

    public async Task Invoke(HttpContext context)
    {
        string? correlationId = context.Request.Headers[CorrelationHeaderName].FirstOrDefault();
        correlationId ??= Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        context.Response.Headers[CorrelationHeaderName] = correlationId;
        context.Items[CorrelationHeaderName] = correlationId;

        string? userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst("sub")?.Value ?? context.User.Identity?.Name
            : null;

        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("requestId", correlationId))
        using (LogContext.PushProperty("route", context.Request.Path.Value ?? string.Empty))
        using (LogContext.PushProperty("method", context.Request.Method))
        using (LogContext.PushProperty("userId", userId ?? string.Empty))
        {
            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["Path"] = context.Request.Path.Value,
                ["Method"] = context.Request.Method
            }))
            {
                await next(context);

                stopwatch.Stop();
                logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
