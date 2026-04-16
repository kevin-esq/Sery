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
            using (LogContext.PushProperty("errorCode", ErrorCatalog.UnexpectedError.Code))
            using (LogContext.PushProperty("messageKey", ErrorCatalog.UnexpectedError.MessageKey))
            {
                logger.LogError(exception, "Unhandled exception while processing request.");
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            IApiProblemDetailsFactory problemDetailsFactory = context.RequestServices.GetRequiredService<IApiProblemDetailsFactory>();

            ProblemDetails problem = problemDetailsFactory.CreateUnexpectedErrorProblem(
                context,
                ErrorCatalog.UnexpectedError);

            if (apiOptions.Value.IncludeExceptionDetails)
            {
                problem = problemDetailsFactory.CreateUnexpectedErrorProblem(
                    context,
                    ErrorCatalog.UnexpectedError,
                    exception.Message);
            }

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
