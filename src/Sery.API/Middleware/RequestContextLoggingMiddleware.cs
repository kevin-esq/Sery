using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Sery.API.Middleware;

public sealed class RequestContextLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestContextLoggingMiddleware> logger)
{
    private const string CorrelationHeaderName = "X-Correlation-ID";

    public async Task Invoke(HttpContext context)
    {
        string correlationId = GetCorrelationId(context);

        context.Response.Headers[CorrelationHeaderName] = correlationId;

        string? userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : "anonymous";

        var scopeItems = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["UserId"] = userId!,
            ["RemoteIp"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        using (logger.BeginScope(scopeItems))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await next(context);

                stopwatch.Stop();

                LogLevel level = context.Response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Information;

                logger.Log(level,
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "An unhandled exception occurred during {Method} {Path}",
                    context.Request.Method, context.Request.Path.Value);
                throw;
            }
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationHeaderName, out StringValues headerId))
        {
            return headerId.FirstOrDefault()!;
        }

        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }
}
