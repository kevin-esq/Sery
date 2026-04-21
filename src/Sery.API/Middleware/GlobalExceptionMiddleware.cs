using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Sery.API.Common;
using Sery.API.Configuration;

namespace Sery.API.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IOptions<ApiFoundationOptions> apiOptions)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            using (LogContext.PushProperty("ErrorCode", ErrorCatalog.UnexpectedError.Code))
            {
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            IApiProblemDetailsFactory problemDetailsFactory = context.RequestServices.GetRequiredService<IApiProblemDetailsFactory>();

            string? detail = apiOptions.Value.IncludeExceptionDetails
                ? exception.ToString()
                : null;

            ProblemDetails problem = problemDetailsFactory.CreateUnexpectedErrorProblem(
                context,
                ErrorCatalog.UnexpectedError,
                detail);

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
