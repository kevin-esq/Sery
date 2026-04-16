using Sery.API.Middleware;

namespace Sery.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiFoundation(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();

        return app;
    }
}
